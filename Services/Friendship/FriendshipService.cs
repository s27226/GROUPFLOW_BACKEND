using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Data;
using GROUPFLOW.DTOs;
using GROUPFLOW.Models;

namespace GROUPFLOW.Services.Friendship;

/// <summary>
/// Service implementation for friendship operations
/// Extracts business logic from GraphQL mutations/queries
/// </summary>
public class FriendshipService : IFriendshipService
{
    private readonly AppDbContext _context;

    public FriendshipService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetUserFriendsAsync(int userId)
    {
        return await _context.Users
            .Where(u => _context.Friendships
                .Any(f => (f.UserId == userId && f.FriendId == u.Id && f.IsAccepted) ||
                         (f.FriendId == userId && f.UserId == u.Id && f.IsAccepted)))
            .ToListAsync();
    }

    public async Task<FriendshipStatusDto> GetFriendshipStatusAsync(int userId, int friendId)
    {
        var friendship = await _context.Friendships
            .FirstOrDefaultAsync(f => 
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.FriendId == userId && f.UserId == friendId));

        if (friendship == null)
        {
            return new FriendshipStatusDto 
            { 
                Status = "none",
                CanSendRequest = true,
                CanAcceptRequest = false
            };
        }

        if (friendship.IsAccepted)
        {
            return new FriendshipStatusDto 
            { 
                Status = "friends",
                CanSendRequest = false,
                CanAcceptRequest = false
            };
        }

        if (friendship.UserId == userId)
        {
            return new FriendshipStatusDto 
            { 
                Status = "pending_sent",
                CanSendRequest = false,
                CanAcceptRequest = false
            };
        }

        return new FriendshipStatusDto 
        { 
            Status = "pending_received",
            CanSendRequest = false,
            CanAcceptRequest = true
        };
    }

    public async Task<bool> RemoveFriendshipAsync(int userId, int friendId)
    {
        // Use transaction for consistency
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // Remove both directions of the friendship
            var friendships = await _context.Friendships
                .Where(f => (f.UserId == userId && f.FriendId == friendId) ||
                           (f.UserId == friendId && f.FriendId == userId))
                .ToListAsync();

            if (!friendships.Any())
            {
                return false;
            }

            _context.Friendships.RemoveRange(friendships);
            
            // Single SaveChangesAsync call - consistent and atomic
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Models.Friendship> AcceptFriendRequestAsync(int userId, int friendRequestId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var friendRequest = await _context.FriendRequests
                .FirstOrDefaultAsync(fr => fr.Id == friendRequestId && fr.RequesteeId == userId);

            if (friendRequest == null)
            {
                throw new InvalidOperationException("Friend request not found");
            }

            // Create friendship
            var friendship = new Models.Friendship
            {
                UserId = friendRequest.RequesterId,
                FriendId = friendRequest.RequesteeId,
                IsAccepted = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.Add(friendship);

            // Remove the friend request
            _context.FriendRequests.Remove(friendRequest);

            // Single SaveChangesAsync - transactional consistency
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return friendship;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
