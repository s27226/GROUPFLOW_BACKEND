using GroupFlow_BACKEND.Models;
using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

public class FriendshipMutation
{
    public bool RemoveFriend(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new GraphQLException("User not authenticated");
        }

        // Remove both directions of the friendship
        var friendships = context.Friendships
            .Where(f => (f.UserId == userId && f.FriendId == friendId) ||
                       (f.UserId == friendId && f.FriendId == userId))
            .ToList();

        if (!friendships.Any())
        {
            throw new GraphQLException("Friendship not found");
        }

        context.Friendships.RemoveRange(friendships);
        context.SaveChanges();

        return true;
    }
}
