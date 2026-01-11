using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services.Friendship;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for friendship operations.
/// </summary>
public class FriendshipMutation
{
    private readonly ILogger<FriendshipMutation> _logger;

    public FriendshipMutation(ILogger<FriendshipMutation> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Remove friendship - uses service layer for business logic with transaction support
    /// </summary>
    public async Task<bool> RemoveFriend(
        [Service] IFriendshipService friendshipService,
        ClaimsPrincipal claimsPrincipal,
        int friendId,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();
        var result = await friendshipService.RemoveFriendshipAsync(userId, friendId);

        if (!result)
            throw new EntityNotFoundException("Friendship");

        _logger.LogInformation("User {UserId} removed friend {FriendId}", userId, friendId);
        return true;
    }
}

