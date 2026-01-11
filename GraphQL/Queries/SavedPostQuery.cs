using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Data;
using GROUPFLOW.Models;

namespace GROUPFLOW.GraphQL.Queries;

public class SavedPostQuery
{
    [GraphQLName("savedposts")]
    [UseProjection]
    [UseSorting]
    public IQueryable<Post> GetSavedPosts(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return context.SavedPosts
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .Select(sp => sp.Post);
    }

    [GraphQLName("isPostSaved")]
    public async Task<bool> IsPostSaved(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return await context.SavedPosts
            .AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);
    }
}
