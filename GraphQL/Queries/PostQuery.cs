using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class PostQuery
{
    [GraphQLName("allposts")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPosts(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        return context.Posts
            .Include(p => p.Likes)
            .Where(p => p.Public || p.UserId == userId);
    }

    [GraphQLName("allpostsbyid")]
    [UseProjection]
    public Post? GetPostsById(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        return context.Posts
            .Include(p => p.Likes)
            .FirstOrDefault(p => (p.Public || p.UserId == userId) && p.Id == id);
    }

    [GraphQLName("isPostLikedByUser")]
    public async Task<bool> IsPostLikedByUser(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        
        return await context.PostLikes
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }
}

