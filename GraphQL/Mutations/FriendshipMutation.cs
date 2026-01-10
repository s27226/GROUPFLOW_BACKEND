using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services.Friendship;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class FriendshipMutation
{
    /// <summary>
    /// Remove friendship - now uses service layer for business logic
    /// Implements transaction for consistency
    /// </summary>
    public async Task<bool> RemoveFriend(
        [Service] IFriendshipService friendshipService,
        ClaimsPrincipal claimsPrincipal,
        int friendId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new GraphQLException("User not authenticated");
        }

        var result = await friendshipService.RemoveFriendshipAsync(userId, friendId);
        
        if (!result)
        {
            throw new GraphQLException("Friendship not found");
        }

        return true;
    }
}

