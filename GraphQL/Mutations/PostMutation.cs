using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.GraphQL.Inputs;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

public class PostMutation
{
    [GraphQLName("createPost")]
    public async Task<Post> CreatePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        PostInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // If projectId is provided, verify the user is a member (collaborator) or owner of that project
        if (input.ProjectId.HasValue)
        {
            var project = await context.Projects
                .Include(p => p.Collaborators)
                .FirstOrDefaultAsync(p => p.Id == input.ProjectId.Value);

            if (project == null)
            {
                throw new GraphQLException("Project not found");
            }

            // Check if user is owner OR a collaborator
            var isOwner = project.OwnerId == userId;
            var isCollaborator = project.Collaborators.Any(c => c.UserId == userId);

            if (!isOwner && !isCollaborator)
            {
                throw new GraphQLException("You are not a member of this project");
            }
        }

        // If sharedPostId is provided, verify the post exists
        if (input.SharedPostId.HasValue)
        {
            var sharedPost = await context.Posts.FindAsync(input.SharedPostId.Value);
            if (sharedPost == null)
            {
                throw new GraphQLException("Shared post not found");
            }
        }

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

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        // Reload the post with User navigation property
        await context.Entry(post)
            .Reference(p => p.User)
            .LoadAsync();

        return post;
    }

    [GraphQLName("likePost")]
    public async Task<PostLike> LikePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if post exists
        var post = await context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == postId);
        
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // Check if already liked
        var existingLike = await context.PostLikes
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

        if (existingLike != null)
        {
            throw new GraphQLException("Post is already liked");
        }

        var postLike = new PostLike
        {
            UserId = userId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow
        };

        context.PostLikes.Add(postLike);

        // Create notification for the post owner (but not if they liked their own post)
        if (post.UserId != userId)
        {
            var liker = await context.Users.FindAsync(userId);
            var notification = new Notification
            {
                UserId = post.UserId,
                Type = "POST_LIKE",
                Message = $"{liker?.Nickname ?? liker?.Name} liked your post",
                ActorUserId = userId,
                PostId = postId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();

        return postLike;
    }

    [GraphQLName("unlikePost")]
    public async Task<bool> UnlikePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var postLike = await context.PostLikes
            .FirstOrDefaultAsync(pl => pl.UserId == userId && pl.PostId == postId);

        if (postLike == null)
        {
            throw new GraphQLException("Post is not liked");
        }

        context.PostLikes.Remove(postLike);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("addComment")]
    public async Task<PostComment> AddComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string content,
        int? parentCommentId = null)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if post exists
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // If replying to a comment, check if parent comment exists
        if (parentCommentId.HasValue)
        {
            var parentComment = await context.PostComments.FindAsync(parentCommentId.Value);
            if (parentComment == null)
            {
                throw new GraphQLException("Parent comment not found");
            }
        }

        var comment = new PostComment
        {
            UserId = userId,
            PostId = postId,
            Content = content,
            ParentCommentId = parentCommentId,
            CreatedAt = DateTime.UtcNow
        };

        context.PostComments.Add(comment);
        await context.SaveChangesAsync();

        // Reload the comment with User navigation property
        await context.Entry(comment)
            .Reference(c => c.User)
            .LoadAsync();

        return comment;
    }

    [GraphQLName("deleteComment")]
    public async Task<bool> DeleteComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var comment = await context.PostComments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
        {
            throw new GraphQLException("Comment not found");
        }

        // Only the comment author can delete it
        if (comment.UserId != userId)
        {
            throw new GraphQLException("You are not authorized to delete this comment");
        }

        // Note: Cascade delete will remove all replies and likes
        context.PostComments.Remove(comment);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("likeComment")]
    public async Task<PostCommentLike> LikeComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if comment exists
        var comment = await context.PostComments.FindAsync(commentId);
        if (comment == null)
        {
            throw new GraphQLException("Comment not found");
        }

        // Check if already liked
        var existingLike = await context.PostCommentLikes
            .FirstOrDefaultAsync(pcl => pcl.UserId == userId && pcl.PostCommentId == commentId);

        if (existingLike != null)
        {
            throw new GraphQLException("Comment is already liked");
        }

        var commentLike = new PostCommentLike
        {
            UserId = userId,
            PostCommentId = commentId,
            CreatedAt = DateTime.UtcNow
        };

        context.PostCommentLikes.Add(commentLike);
        await context.SaveChangesAsync();

        return commentLike;
    }

    [GraphQLName("unlikeComment")]
    public async Task<bool> UnlikeComment(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var commentLike = await context.PostCommentLikes
            .FirstOrDefaultAsync(pcl => pcl.UserId == userId && pcl.PostCommentId == commentId);

        if (commentLike == null)
        {
            throw new GraphQLException("Comment is not liked");
        }

        context.PostCommentLikes.Remove(commentLike);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("sharePost")]
    public async Task<Post> SharePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string? content = null,
        int? projectId = null)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if original post exists
        var originalPost = await context.Posts.FindAsync(postId);
        if (originalPost == null)
        {
            throw new GraphQLException("Post not found");
        }

        var sharedPost = new Post
        {
            UserId = userId,
            ProjectId = projectId,
            Title = "Shared Post",
            Description = content ?? "",
            Content = content ?? $"Shared a post",
            SharedPostId = postId,
            Public = true,
            Created = DateTime.UtcNow
        };

        context.Posts.Add(sharedPost);
        await context.SaveChangesAsync();

        return sharedPost;
    }

    [GraphQLName("reportPost")]
    public async Task<PostReport> ReportPost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        ReportPostInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if post exists
        var post = await context.Posts.FindAsync(input.PostId);
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // Check if user already reported this post
        var existingReport = await context.PostReports
            .FirstOrDefaultAsync(pr => pr.PostId == input.PostId && pr.ReportedBy == userId && !pr.IsResolved);

        if (existingReport != null)
        {
            throw new GraphQLException("You have already reported this post");
        }

        var report = new PostReport
        {
            PostId = input.PostId,
            ReportedBy = userId,
            Reason = input.Reason,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        context.PostReports.Add(report);
        await context.SaveChangesAsync();

        // Reload with navigation properties
        await context.Entry(report)
            .Reference(r => r.Post)
            .LoadAsync();
        await context.Entry(report)
            .Reference(r => r.ReportedByUser)
            .LoadAsync();

        return report;
    }

    [GraphQLName("deleteReportedPost")]
    public async Task<bool> DeleteReportedPost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if user is moderator
        var user = await context.Users.FindAsync(userId);
        if (user == null || !user.IsModerator)
        {
            throw new GraphQLException("You are not authorized to delete reported posts");
        }

        // Check if post exists
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // Mark all reports for this post as resolved
        var reports = await context.PostReports
            .Where(pr => pr.PostId == postId && !pr.IsResolved)
            .ToListAsync();

        foreach (var report in reports)
        {
            report.IsResolved = true;
        }

        // Delete the post (cascade will handle related data)
        context.Posts.Remove(post);
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("discardReport")]
    public async Task<bool> DiscardReport(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int reportId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if user is moderator
        var user = await context.Users.FindAsync(userId);
        if (user == null || !user.IsModerator)
        {
            throw new GraphQLException("You are not authorized to discard reports");
        }

        // Find the report
        var report = await context.PostReports.FindAsync(reportId);
        if (report == null)
        {
            throw new GraphQLException("Report not found");
        }

        // Mark as resolved without deleting the post
        report.IsResolved = true;
        await context.SaveChangesAsync();

        return true;
    }
}
