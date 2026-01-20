using System.Security.Claims;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Users.Entities;
using GROUPFLOW.Features.Friendships.Entities;
using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Features.Friendships.GraphQL.Queries;

public class BlockedUserQuery
{
    [GraphQLName("blockedusers")]
    public async Task<List<User>> GetBlockedUsers(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Get users that the current user has blocked
        var blockedUserIds = await context.BlockedUsers
            .Where(bu => bu.UserId == userId)
            .Select(bu => bu.BlockedUserId)
            .ToListAsync();
            
        return await context.Users
            .Include(u => u.ProfilePicBlob)
            .Where(u => blockedUserIds.Contains(u.Id))
            .ToListAsync();
    }

    [GraphQLName("isblockedby")]
    public bool IsBlockedBy(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int userId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int currentUserId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check if the specified user has blocked the current user
        return context.BlockedUsers
            .Any(bu => bu.UserId == userId && bu.BlockedUserId == currentUserId);
    }

    [GraphQLName("hasblocked")]
    public bool HasBlocked(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int userId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int currentUserId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Check if the current user has blocked the specified user
        return context.BlockedUsers
            .Any(bu => bu.UserId == currentUserId && bu.BlockedUserId == userId);
    }
}
