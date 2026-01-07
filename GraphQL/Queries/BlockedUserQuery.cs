using System.Security.Claims;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class BlockedUserQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BlockedUserQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("blockedusers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<UserResponse> GetBlockedUsers()
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Get users that the current user has blocked
        return _context.Users
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => _context.BlockedUsers
                .Any(bu => bu.UserId == userId && bu.BlockedUserId == u.Id))
            .Select(u => UserResponse.FromUser(u));
    }

    [GraphQLName("isblockedby")]
    public bool IsBlockedBy(int userId)
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int currentUserId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check if the specified user has blocked the current user
        return _context.BlockedUsers
            .Any(bu => bu.UserId == userId && bu.BlockedUserId == currentUserId);
    }

    [GraphQLName("hasblocked")]
    public bool HasBlocked(int userId)
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int currentUserId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check if the current user has blocked the specified user
        return _context.BlockedUsers
            .Any(bu => bu.UserId == currentUserId && bu.BlockedUserId == userId);
    }
}
