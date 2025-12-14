using System.Security.Claims;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class FriendshipQuery
{
    [GraphQLName("myfriends")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetMyFriends(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return context.Users
            .Where(u => context.Friendships
                .Any(f => (f.UserId == userId && f.FriendId == u.Id && f.IsAccepted) ||
                         (f.FriendId == userId && f.UserId == u.Id && f.IsAccepted)));
    }

    [GraphQLName("friendshipstatus")]
    public string GetFriendshipStatus(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int friendId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var friendship = context.Friendships
            .FirstOrDefault(f => 
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.FriendId == userId && f.UserId == friendId));

        if (friendship == null) return "none";
        if (friendship.IsAccepted) return "friends";
        if (friendship.UserId == userId) return "pending_sent";
        return "pending_received";
    }
}