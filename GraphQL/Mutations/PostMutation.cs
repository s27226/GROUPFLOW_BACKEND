using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Services.Post;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class PostMutation
{
    /// <summary>
    /// Create post - now uses service layer with transaction support
    /// </summary>
    [GraphQLName("createPost")]
    public async Task<Post> CreatePost(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        PostInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return await postService.CreatePostAsync(
            userId,
            input.ProjectId,
            input.Title ?? "Post",
            input.Description ?? "",
            input.Content,
            input.ImageUrl,
            input.SharedPostId,
            input.IsPublic
        );
    }

    /// <summary>
    /// Like post - now uses service layer with transaction support
    /// </summary>
    [GraphQLName("likePost")]
    public async Task<PostLike> LikePost(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return await postService.LikePostAsync(userId, postId);
    }

    /// <summary>
    /// Unlike post - now uses service layer
    /// </summary>
    [GraphQLName("unlikePost")]
    public async Task<bool> UnlikePost(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return await postService.UnlikePostAsync(userId, postId);
    }

    /// <summary>
    /// Add comment - now uses service layer with transaction support
    /// </summary>
    [GraphQLName("addComment")]
    public async Task<PostComment> AddComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string content,
        int? parentCommentId = null)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await postService.AddCommentAsync(userId, postId, content, parentCommentId);
    }

    /// <summary>
    /// Delete comment - now uses service layer with transaction support
    /// </summary>
    [GraphQLName("deleteComment")]
    public async Task<bool> DeleteComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await postService.DeleteCommentAsync(userId, commentId);
    }

    /// <summary>
    /// Like comment - now uses service layer with transaction support
    /// </summary>
    [GraphQLName("likeComment")]
    public async Task<PostLike> LikeComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await postService.LikeCommentAsync(userId, commentId);
    }

    /// <summary>
    /// Unlike comment - now uses service layer
    /// </summary>
    [GraphQLName("unlikeComment")]
    public async Task<bool> UnlikeComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await postService.UnlikeCommentAsync(userId, commentId);
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
