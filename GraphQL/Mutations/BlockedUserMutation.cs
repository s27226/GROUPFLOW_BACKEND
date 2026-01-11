using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for blocked user operations.
/// </summary>
public class BlockedUserMutation
{
    private readonly ILogger<BlockedUserMutation> _logger;

    public BlockedUserMutation(ILogger<BlockedUserMutation> logger)
    {
        _logger = logger;
    }

    public async Task<BlockedUser> BlockUser(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int userIdToBlock,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        if (userId == userIdToBlock)
            throw BusinessRuleException.CannotBlockYourself();

        // Check if users are friends
        var areFriends = await context.Friendships
            .AnyAsync(f =>
                ((f.UserId == userId && f.FriendId == userIdToBlock) ||
                 (f.UserId == userIdToBlock && f.FriendId == userId)) &&
                f.IsAccepted, ct);

        if (areFriends)
            throw new BusinessRuleException("You cannot block a friend. Please remove them as a friend first.");

        // Check if already blocked
        var existingBlock = await context.BlockedUsers
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BlockedUserId == userIdToBlock, ct);

        if (existingBlock != null)
            throw new DuplicateEntityException("BlockedUser");

        var blockedUser = new BlockedUser
        {
            UserId = userId,
            BlockedUserId = userIdToBlock,
            BlockedAt = DateTime.UtcNow
        };

        context.BlockedUsers.Add(blockedUser);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} blocked user {BlockedUserId}", userId, userIdToBlock);
        return blockedUser;
    }

    public async Task<bool> UnblockUser(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int userIdToUnblock,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var blockedUser = await context.BlockedUsers
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BlockedUserId == userIdToUnblock, ct)
            ?? throw new EntityNotFoundException("BlockedUser");

        context.BlockedUsers.Remove(blockedUser);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} unblocked user {UnblockedUserId}", userId, userIdToUnblock);
        return true;
    }
}
