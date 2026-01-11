using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for saved post operations.
/// </summary>
public class SavedPostMutation
{
    private readonly ILogger<SavedPostMutation> _logger;

    public SavedPostMutation(ILogger<SavedPostMutation> logger)
    {
        _logger = logger;
    }

    [GraphQLName("savePost")]
    public async Task<SavedPost> SavePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        // Check if post exists
        var post = await context.Posts.FindAsync(new object[] { postId }, ct)
            ?? throw EntityNotFoundException.Post(postId);

        // Check if already saved
        var existingSave = await context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId, ct);

        if (existingSave != null)
            throw DuplicateEntityException.AlreadySaved();

        var savedPost = new SavedPost
        {
            UserId = userId,
            PostId = postId,
            SavedAt = DateTime.UtcNow
        };

        context.SavedPosts.Add(savedPost);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} saved post {PostId}", userId, postId);
        return savedPost;
    }

    [GraphQLName("unsavePost")]
    public async Task<bool> UnsavePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var savedPost = await context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId, ct)
            ?? throw new EntityNotFoundException("SavedPost");

        context.SavedPosts.Remove(savedPost);
        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} unsaved post {PostId}", userId, postId);
        return true;
    }
}
