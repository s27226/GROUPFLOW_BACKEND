using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Services;

public class BlockedUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BlockedUserService> _logger;

    public BlockedUserService(AppDbContext context, ILogger<BlockedUserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BlockedUser> BlockUser(int userId, int userIdToBlock)
    {

        _logger.LogInformation("User {UserId} is attempting to block user {UserIdToBlock}", userId, userIdToBlock);
        if (userId == userIdToBlock)
        {
            _logger.LogWarning("User {UserId} attempted to block themselves", userId);

            throw new GraphQLException("You cannot block yourself");
        }




        // Check if users are friends
        var areFriends = await _context.Friendships
            .AnyAsync(f =>
                ((f.UserId == userId && f.FriendId == userIdToBlock) ||
                 (f.UserId == userIdToBlock && f.FriendId == userId)) &&
                f.IsAccepted);

        if (areFriends)
        {
            _logger.LogWarning("User {UserId} attempted to block friend {UserIdToBlock}", userId, userIdToBlock);

            throw new GraphQLException("You cannot block a friend. Please remove them as a friend first.");
        }


        // Check if already blocked
        var existingBlock = await _context.BlockedUsers
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BlockedUserId == userIdToBlock);

        if (existingBlock != null)
        {
            _logger.LogWarning("User {UserId} attempted to block already blocked user {UserIdToBlock}", userId,
                userIdToBlock);
            throw new GraphQLException("User is already blocked");
        }

        var blockedUser = new BlockedUser
        {
            UserId = userId,
            BlockedUserId = userIdToBlock,
            BlockedAt = DateTime.UtcNow
        };

        _context.BlockedUsers.Add(blockedUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} successfully blocked user {UserIdToBlock}", userId, userIdToBlock);

        return blockedUser;
    }

    public async Task<bool> UnblockUser(int userId, int userIdToUnblock)
    {
        _logger.LogInformation("User {UserId} is attempting to unblock user {UserIdToUnblock}", userId,
            userIdToUnblock);

        var blockedUser = await _context.BlockedUsers
            .FirstOrDefaultAsync(bu => bu.UserId == userId && bu.BlockedUserId == userIdToUnblock);

        if (blockedUser == null)
        {
            _logger.LogWarning("User {UserId} attempted to unblock user {UserIdToUnblock} who is not blocked", userId,
                userIdToUnblock);
            throw new GraphQLException("User is not blocked");
        }

        _context.BlockedUsers.Remove(blockedUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} successfully unblocked user {UserIdToUnblock}", userId, userIdToUnblock);

        return true;
    }
}