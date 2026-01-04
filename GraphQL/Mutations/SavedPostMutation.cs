using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

public class SavedPostMutation
{
    [GraphQLName("savePost")]
    public async Task<SavedPost> SavePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        // Check if post exists
        var post = await context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new GraphQLException("Post not found");
        }

        // Check if already saved
        var existingSave = await context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (existingSave != null)
        {
            throw new GraphQLException("Post is already saved");
        }

        var savedPost = new SavedPost
        {
            UserId = userId,
            PostId = postId,
            SavedAt = DateTime.UtcNow
        };

        context.SavedPosts.Add(savedPost);
        await context.SaveChangesAsync();

        return savedPost;
    }

    [GraphQLName("unsavePost")]
    public async Task<bool> UnsavePost(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        var savedPost = await context.SavedPosts
            .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.PostId == postId);

        if (savedPost == null)
        {
            throw new GraphQLException("Post is not saved");
        }

        context.SavedPosts.Remove(savedPost);
        await context.SaveChangesAsync();

        return true;
    }
}
