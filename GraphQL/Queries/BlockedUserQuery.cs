using System.Security.Claims;
using GROUPFLOW.Data;
using GROUPFLOW.Models;
using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.GraphQL.Queries;

public class BlockedUserQuery
{
    [GraphQLName("blockedusers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetBlockedUsers(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Get users that the current user has blocked
        return context.Users
            .Where(u => context.BlockedUsers
                .Any(bu => bu.UserId == userId && bu.BlockedUserId == u.Id));
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
