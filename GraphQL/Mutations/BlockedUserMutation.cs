using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class BlockedUserMutation
{
    public async Task<BlockedUser> BlockUser(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int userIdToBlock)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new GraphQLException("User not authenticated");
        }

        // Cannot block yourself
        if (userId == userIdToBlock)
        {
            throw new GraphQLException("You cannot block yourself");
        }

        // Check if users are friends
        var areFriends = await context.Friendships
            .AnyAsync(f => 
                ((f.UserId == userId && f.FriendId == userIdToBlock) ||
                 (f.UserId == userIdToBlock && f.FriendId == userId)) &&
                f.IsAccepted);

        if (areFriends)
        {
            throw new GraphQLException("You cannot block a friend. Please remove them as a friend first.");
        }

        // Check if already blocked
        var existingBlock = await context.BlockedUsers
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BlockedUserId == userIdToBlock);

        if (existingBlock != null)
        {
            throw new GraphQLException("User is already blocked");
        }

        // Create the block
        var blockedUser = new BlockedUser
        {
            UserId = userId,
            BlockedUserId = userIdToBlock,
            BlockedAt = DateTime.UtcNow
        };

        context.BlockedUsers.Add(blockedUser);
        await context.SaveChangesAsync();

        return blockedUser;
    }

    public async Task<bool> UnblockUser(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int userIdToUnblock)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new GraphQLException("User not authenticated");
        }

        var blockedUser = await context.BlockedUsers
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BlockedUserId == userIdToUnblock);

        if (blockedUser == null)
        {
            throw new GraphQLException("User is not blocked");
        }

        context.BlockedUsers.Remove(blockedUser);
        await context.SaveChangesAsync();

        return true;
    }
}
