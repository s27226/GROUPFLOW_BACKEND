using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class FriendshipMutation
{
    private readonly FriendshipService _friendshipService;

    public FriendshipMutation(FriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    public async Task<bool> RemoveFriend(
        ClaimsPrincipal claimsPrincipal,
        int friendId,
        CancellationToken cancellationToken)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new NotAuthenticatedException("User not authenticated");
        }

        return await _friendshipService.RemoveFriendAsync(userId, friendId, cancellationToken);
    }
}

// --- Serwis biznesowy ---
public class FriendshipService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FriendshipService> _logger;

    public FriendshipService(AppDbContext context, ILogger<FriendshipService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> RemoveFriendAsync(int userId, int friendId, CancellationToken ct)
    {
        var friendships = await _context.Friendships
            .Where(f => (f.UserId == userId && f.FriendId == friendId) ||
                        (f.UserId == friendId && f.FriendId == userId))
            .ToListAsync(ct);

        if (!friendships.Any())
        {
            throw new FriendshipNotFoundException($"Friendship between {userId} and {friendId} not found");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(ct);

        _context.Friendships.RemoveRange(friendships);
        await _context.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        _logger.LogInformation("User {UserId} removed friendship with {FriendId}", userId, friendId);

        return true;
    }
}

// --- Dedykowane wyjątki ---
public class NotAuthenticatedException : GraphQLException
{
    public NotAuthenticatedException(string message) : base(message) { }
}

public class FriendshipNotFoundException : GraphQLException
{
    public FriendshipNotFoundException(string message) : base(message) { }
}