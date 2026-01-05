using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.Services;

public class PostService
{
    private readonly AppDbContext _context;
    private readonly ILogger<PostService> _logger;

    public PostService(AppDbContext context, ILogger<PostService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<Post> CreatePost(ClaimsPrincipal user, PostInput input)
    {
        int userId = GetUserId(user);

        if (input.ProjectId.HasValue)
        {
            var project = await _context.Projects
                .Include(p => p.Collaborators)
                .FirstOrDefaultAsync(p => p.Id == input.ProjectId.Value);

            if (project == null)
                throw new GraphQLException("Project not found");

            if (project.OwnerId != userId &&
                !project.Collaborators.Any(c => c.UserId == userId))
                throw new GraphQLException("You are not a member of this project");
        }

        if (input.SharedPostId.HasValue &&
            !await _context.Posts.AnyAsync(p => p.Id == input.SharedPostId))
            throw new GraphQLException("Shared post not found");

        var post = new Post
        {
            UserId = userId,
            ProjectId = input.ProjectId,
            Title = input.Title ?? "Post",
            Description = input.Description ?? "",
            Content = input.Content,
            ImageUrl = input.ImageUrl,
            SharedPostId = input.SharedPostId,
            Public = input.IsPublic,
            Created = DateTime.UtcNow
        };

        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        await _context.Entry(post).Reference(p => p.User).LoadAsync();

        _logger.LogInformation("User {UserId} created post {PostId}", userId, post.Id);

        return post;
    }

    public async Task<PostLike> LikePost(ClaimsPrincipal user, int postId)
    {
        int userId = GetUserId(user);

        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == postId);

        if (post == null)
            throw new GraphQLException("Post not found");

        if (await _context.PostLikes.AnyAsync(l => l.UserId == userId && l.PostId == postId))
            throw new GraphQLException("Post is already liked");

        var like = new PostLike
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };

        _context.PostLikes.Add(like);

        if (post.UserId != userId)
        {
            var liker = await _context.Users.FindAsync(userId);
            _context.Notifications.Add(new Notification
            {
                UserId = post.UserId,
                Type = "POST_LIKE",
                Message = $"{liker?.Nickname ?? liker?.Name} liked your post",
                ActorUserId = userId,
                PostId = postId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("User {UserId} liked post {PostId}", userId, postId);

        return like;
    }

    public async Task<bool> UnlikePost(ClaimsPrincipal user, int postId)
    {
        int userId = GetUserId(user);

        var like = await _context.PostLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

        if (like == null)
            throw new GraphQLException("Post is not liked");

        _context.PostLikes.Remove(like);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} unliked post {PostId}", userId, postId);
        return true;
    }

    public async Task<PostComment> AddComment(
        ClaimsPrincipal user,
        int postId,
        string content,
        int? parentCommentId)
    {
        int userId = GetUserId(user);

        if (!await _context.Posts.AnyAsync(p => p.Id == postId))
            throw new GraphQLException("Post not found");

        if (parentCommentId.HasValue &&
            !await _context.PostComments.AnyAsync(c => c.Id == parentCommentId))
            throw new GraphQLException("Parent comment not found");

        var comment = new PostComment
        {
            UserId = userId,
            PostId = postId,
            Content = content,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.PostComments.Add(comment);
        await _context.SaveChangesAsync();
        await _context.Entry(comment).Reference(c => c.User).LoadAsync();

        _logger.LogInformation("User {UserId} added comment {CommentId} to post {PostId}", userId, comment.Id, postId);

        return comment;
    }

    public async Task<bool> DeleteComment(ClaimsPrincipal user, int commentId)
    {
        int userId = GetUserId(user);

        var comment = await _context.PostComments.FindAsync(commentId);
        if (comment == null)
            throw new GraphQLException("Comment not found");

        if (comment.UserId != userId)
            throw new GraphQLException("You are not authorized to delete this comment");

        _context.PostComments.Remove(comment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted comment {CommentId}", userId, commentId);
        return true;
    }

    public async Task<PostCommentLike> LikeComment(ClaimsPrincipal user, int commentId)
    {
        int userId = GetUserId(user);

        if (!await _context.PostComments.AnyAsync(c => c.Id == commentId))
            throw new GraphQLException("Comment not found");

        if (await _context.PostCommentLikes.AnyAsync(l => l.UserId == userId && l.PostCommentId == commentId))
            throw new GraphQLException("Comment is already liked");

        var like = new PostCommentLike
        {
            UserId = userId,
            PostCommentId = commentId,
            CreatedAt = DateTime.UtcNow
        };

        _context.PostCommentLikes.Add(like);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} liked comment {CommentId}", userId, commentId);
        return like;
    }

    public async Task<bool> UnlikeComment(ClaimsPrincipal user, int commentId)
    {
        int userId = GetUserId(user);

        var like = await _context.PostCommentLikes
            .FirstOrDefaultAsync(l => l.UserId == userId && l.PostCommentId == commentId);

        if (like == null)
            throw new GraphQLException("Comment is not liked");

        _context.PostCommentLikes.Remove(like);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} unliked comment {CommentId}", userId, commentId);
        return true;
    }

    public async Task<Post> SharePost(ClaimsPrincipal user, int postId, string? content, int? projectId)
    {
        int userId = GetUserId(user);

        var originalPost = await _context.Posts.FindAsync(postId);
        if (originalPost == null)
            throw new GraphQLException("Post not found");

        var sharedPost = new Post
        {
            UserId = userId,
            ProjectId = projectId,
            Title = "Shared Post",
            Description = content ?? "",
            Content = content ?? "Shared a post",
            SharedPostId = postId,
            Public = true,
            Created = DateTime.UtcNow
        };

        _context.Posts.Add(sharedPost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} shared post {PostId} as new post {SharedPostId}", userId, postId, sharedPost.Id);

        return sharedPost;
    }

    public async Task<PostReport> ReportPost(ClaimsPrincipal user, ReportPostInput input)
    {
        int userId = GetUserId(user);

        if (!await _context.Posts.AnyAsync(p => p.Id == input.PostId))
            throw new GraphQLException("Post not found");

        if (await _context.PostReports.AnyAsync(r =>
                r.PostId == input.PostId &&
                r.ReportedBy == userId &&
                !r.IsResolved))
            throw new GraphQLException("You have already reported this post");

        var report = new PostReport
        {
            PostId = input.PostId,
            ReportedBy = userId,
            Reason = input.Reason,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        _context.PostReports.Add(report);
        await _context.SaveChangesAsync();

        await _context.Entry(report).Reference(r => r.Post).LoadAsync();
        await _context.Entry(report).Reference(r => r.ReportedByUser).LoadAsync();

        _logger.LogInformation("User {UserId} reported post {PostId} with report {ReportId}", userId, input.PostId, report.Id);

        return report;
    }

    public async Task<bool> DeleteReportedPost(ClaimsPrincipal user, int postId)
    {
        int userId = GetUserId(user);

        var moderator = await _context.Users.FindAsync(userId);
        if (moderator == null || !moderator.IsModerator)
            throw new GraphQLException("You are not authorized to delete reported posts");

        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
            throw new GraphQLException("Post not found");

        var reports = await _context.PostReports
            .Where(r => r.PostId == postId && !r.IsResolved)
            .ToListAsync();

        foreach (var report in reports)
            report.IsResolved = true;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Moderator {UserId} deleted reported post {PostId}", userId, postId);

        return true;
    }

    public async Task<bool> DiscardReport(ClaimsPrincipal user, int reportId)
    {
        int userId = GetUserId(user);

        var moderator = await _context.Users.FindAsync(userId);
        if (moderator == null || !moderator.IsModerator)
            throw new GraphQLException("You are not authorized to discard reports");

        var report = await _context.PostReports.FindAsync(reportId);
        if (report == null)
            throw new GraphQLException("Report not found");

        report.IsResolved = true;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Moderator {UserId} discarded report {ReportId}", userId, reportId);

        return true;
    }
}
