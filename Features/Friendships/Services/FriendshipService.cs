using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Friendships.DTOs;
using GROUPFLOW.Features.Friendships.Entities;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Friendships.Services;

/// <summary>
/// Service implementation for friendship operations.
/// Extracts business logic from GraphQL mutations/queries.
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

    public async Task<Friendship> AcceptFriendRequestAsync(int userId, int friendRequestId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            var request = await _context.FriendRequests.FindAsync(friendRequestId);
            if (request == null || request.RequesteeId != userId)
            {
                throw new InvalidOperationException("Friend request not found or not authorized");
            }

            // Create bidirectional friendship
            var friendship1 = new Friendship
            {
                UserId = request.RequesterId,
                FriendId = request.RequesteeId,
                IsAccepted = true,
                CreatedAt = DateTime.UtcNow
            };

            var friendship2 = new Friendship
            {
                UserId = request.RequesteeId,
                FriendId = request.RequesterId,
                IsAccepted = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Friendships.AddRange(friendship1, friendship2);
            _context.FriendRequests.Remove(request);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return friendship1;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
