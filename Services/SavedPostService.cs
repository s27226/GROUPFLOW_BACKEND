using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace NAME_WIP_BACKEND.Services;

public class SavedPostService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SavedPostService> _logger;

    public SavedPostService(AppDbContext context, ILogger<SavedPostService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SavedPost> SavePost(int userId, int postId)
    {
        // Sprawdź, czy post istnieje
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
        {
            _logger.LogWarning("User {UserId} tried to save non-existent post {PostId}", userId, postId);
            throw new GraphQLException("Post not found");
        }

        // Sprawdź, czy post nie został już zapisany
        var existingSave = await _context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (existingSave != null)
        {
            _logger.LogWarning("User {UserId} tried to save post {PostId} which is already saved", userId, postId);
            throw new GraphQLException("Post is already saved");
        }

        var savedPost = new SavedPost
        {
            UserId = userId,
            PostId = postId,
            SavedAt = DateTime.UtcNow
        };

        _context.SavedPosts.Add(savedPost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} saved post {PostId}", userId, postId);
        return savedPost;
    }

    public async Task<bool> UnsavePost(int userId, int postId)
    {
        var savedPost = await _context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (savedPost == null)
        {
            _logger.LogWarning("User {UserId} tried to unsave post {PostId} which is not saved", userId, postId);
            throw new GraphQLException("Post is not saved");
        }

        _context.SavedPosts.Remove(savedPost);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} unsaved post {PostId}", userId, postId);
        return true;
    }
}
