using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class SavedPostQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SavedPostQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("savedposts")]
    [UseProjection]
    [UseSorting]
    public IQueryable<PostResponse> GetSavedPosts()
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return _context.SavedPosts
            .Include(sp => sp.Post)
            .ThenInclude(p => p.User)
            .Include(sp => sp.Post)
            .ThenInclude(p => p.Likes)
            .Include(sp => sp.Post)
            .ThenInclude(p => p.Comments)
            .Where(sp => sp.UserId == userId)
            .OrderByDescending(sp => sp.SavedAt)
            .Select(sp => PostResponse.FromPost(sp.Post));
    }

    [GraphQLName("isPostSaved")]
    public async Task<bool> IsPostSaved(int postId)
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));

        return await _context.SavedPosts
            .AnyAsync(sp => sp.UserId == userId && sp.PostId == postId);
    }
}
