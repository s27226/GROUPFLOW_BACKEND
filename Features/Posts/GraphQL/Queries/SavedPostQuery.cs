using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Posts.GraphQL.Queries;

public class SavedPostQuery
{
    [GraphQLName("savedposts")]
    public async Task<List<Post>> GetSavedPosts(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var savedPostIds = await context.SavedPosts
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .Select(sp => sp.PostId)
            .ToListAsync();

        return await context.Posts
            .Include(p => p.User)
                .ThenInclude(u => u.ProfilePicBlob)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
                .ThenInclude(c => c.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .Include(p => p.Comments)
                .ThenInclude(c => c.Replies)
                    .ThenInclude(r => r.User)
                        .ThenInclude(u => u.ProfilePicBlob)
            .Include(p => p.SharedPost)
                .ThenInclude(sp => sp!.User)
                    .ThenInclude(u => u.ProfilePicBlob)
            .Where(p => savedPostIds.Contains(p.Id))
            .ToListAsync();
    }

    [GraphQLName("isPostSaved")]
    public async Task<bool> IsPostSaved(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return await context.SavedPosts
            .AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);
    }
}
