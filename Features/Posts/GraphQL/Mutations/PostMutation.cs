using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Posts.Entities;
using GROUPFLOW.Features.Posts.GraphQL.Inputs;
using GROUPFLOW.Features.Posts.Services;

namespace GROUPFLOW.Features.Posts.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for post operations.
/// Uses service layer for complex operations with transaction support.
/// </summary>
public class PostMutation
{
    private readonly ILogger<PostMutation> _logger;

    public PostMutation(ILogger<PostMutation> logger)
    {
        _logger = logger;
    }

    [GraphQLName("createPost")]
    public async Task<Post> CreatePost(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        PostInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

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

    [GraphQLName("likePost")]
    public async Task<PostLike> LikePost(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();
        return await postService.LikePostAsync(userId, postId);
    }

    [GraphQLName("unlikePost")]
    public async Task<bool> UnlikePost(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();
        return await postService.UnlikePostAsync(userId, postId);
    }

    [GraphQLName("addComment")]
    public async Task<PostComment> AddComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string content,
        int? parentCommentId = null,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();
        return await postService.AddCommentAsync(userId, postId, content, parentCommentId);
    }

    [GraphQLName("deleteComment")]
    public async Task<bool> DeleteComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();
        return await postService.DeleteCommentAsync(userId, commentId);
    }

    [GraphQLName("likeComment")]
    public async Task<PostLike> LikeComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();
        return await postService.LikeCommentAsync(userId, commentId);
    }

    [GraphQLName("unlikeComment")]
    public async Task<bool> UnlikeComment(
        [Service] IPostService postService,
        [Service] IHttpContextAccessor httpContextAccessor,
        int commentId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();
        return await postService.UnlikeCommentAsync(userId, commentId);
    }

    [GraphQLName("sharePost")]
    public async Task<Post> SharePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        string? content = null,
        int? projectId = null,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var originalPost = await context.Posts.FindAsync(new object[] { postId }, ct)
            ?? throw EntityNotFoundException.Post(postId);

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

        context.Posts.Add(sharedPost);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} shared post {PostId}", userId, postId);
        return sharedPost;
    }

    [GraphQLName("reportPost")]
    public async Task<PostReport> ReportPost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        ReportPostInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var post = await context.Posts.FindAsync(new object[] { input.PostId }, ct)
            ?? throw EntityNotFoundException.Post(input.PostId);

        var existingReport = await context.PostReports
            .FirstOrDefaultAsync(pr => pr.PostId == input.PostId && pr.ReportedBy == userId && !pr.IsResolved, ct);

        if (existingReport != null)
            throw new DuplicateEntityException("PostReport", "postId");

        var report = new PostReport
        {
            PostId = input.PostId,
            ReportedBy = userId,
            Reason = input.Reason,
            CreatedAt = DateTime.UtcNow,
            IsResolved = false
        };

        context.PostReports.Add(report);
        await context.SaveChangesAsync(ct);

        await context.Entry(report).Reference(r => r.Post).LoadAsync(ct);
        await context.Entry(report).Reference(r => r.ReportedByUser).LoadAsync(ct);

        _logger.LogInformation("User {UserId} reported post {PostId}", userId, input.PostId);
        return report;
    }

    [GraphQLName("deleteReportedPost")]
    public async Task<bool> DeleteReportedPost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var user = await context.Users.FindAsync(new object[] { userId }, ct)
            ?? throw EntityNotFoundException.User(userId);

        if (!user.IsModerator)
            throw new AuthorizationException("You are not authorized to delete reported posts");

        var post = await context.Posts.FindAsync(new object[] { postId }, ct)
            ?? throw EntityNotFoundException.Post(postId);

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var reports = await context.PostReports
                .Where(pr => pr.PostId == postId && !pr.IsResolved)
                .ToListAsync(ct);

            foreach (var report in reports)
                report.IsResolved = true;

            context.Posts.Remove(post);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });

        _logger.LogInformation("Moderator {UserId} deleted reported post {PostId}", userId, postId);
        return true;
    }

    [GraphQLName("discardReport")]
    public async Task<bool> DiscardReport(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int reportId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var user = await context.Users.FindAsync(new object[] { userId }, ct)
            ?? throw EntityNotFoundException.User(userId);

        if (!user.IsModerator)
            throw new AuthorizationException("You are not authorized to discard reports");

        var report = await context.PostReports.FindAsync(new object[] { reportId }, ct)
            ?? throw new EntityNotFoundException("PostReport", reportId);

        report.IsResolved = true;
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Moderator {UserId} discarded report {ReportId}", userId, reportId);
        return true;
    }
}
