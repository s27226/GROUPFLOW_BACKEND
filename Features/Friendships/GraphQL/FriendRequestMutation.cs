using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Friendships.Entities;
using GROUPFLOW.Features.Friendships.Inputs;

namespace GROUPFLOW.Features.Friendships.GraphQL;

/// <summary>
/// GraphQL mutations for friend request operations.
/// </summary>
public class FriendRequestMutation
{
    private readonly ILogger<FriendRequestMutation> _logger;

    public FriendRequestMutation(ILogger<FriendRequestMutation> logger)
    {
        _logger = logger;
    }

    public async Task<FriendRequest> SendFriendRequest(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int requesteeId,
        CancellationToken ct = default)
    {
        var requesterId = claimsPrincipal.GetAuthenticatedUserId();

        if (requesterId == requesteeId)
            throw BusinessRuleException.CannotFriendYourself();

        // Check if request already exists
        var existingRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.RequesterId == requesterId && fr.RequesteeId == requesteeId, ct);

        if (existingRequest != null)
            throw BusinessRuleException.FriendRequestPending();

        // Check if already friends
        var alreadyFriends = await context.Friendships
            .AnyAsync(f => f.IsAccepted &&
                ((f.UserId == requesterId && f.FriendId == requesteeId) ||
                 (f.UserId == requesteeId && f.FriendId == requesterId)), ct);

        if (alreadyFriends)
            throw BusinessRuleException.AlreadyFriends();

        var request = new FriendRequest
        {
            RequesterId = requesterId,
            RequesteeId = requesteeId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(72)
        };

        context.FriendRequests.Add(request);
        await context.SaveChangesAsync(ct);

        // Load navigation properties
        await context.Entry(request).Reference(r => r.Requester).LoadAsync(ct);
        await context.Entry(request).Reference(r => r.Requestee).LoadAsync(ct);

        _logger.LogInformation("User {RequesterId} sent friend request to {RequesteeId}", requesterId, requesteeId);
        return request;
    }

    public async Task<bool> AcceptFriendRequest(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var request = await context.FriendRequests.FindAsync(new object[] { friendRequestId }, ct)
            ?? throw new EntityNotFoundException("FriendRequest", friendRequestId);

        if (request.RequesteeId != userId)
            throw new AuthorizationException("You can only accept friend requests sent to you");

        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            // Create bidirectional friendship
            context.Friendships.AddRange(
                new Friendship
                {
                    UserId = request.RequesterId,
                    FriendId = request.RequesteeId,
                    IsAccepted = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Friendship
                {
                    UserId = request.RequesteeId,
                    FriendId = request.RequesterId,
                    IsAccepted = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            context.FriendRequests.Remove(request);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);

            _logger.LogInformation("User {UserId} accepted friend request {RequestId}", userId, friendRequestId);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<bool> RejectFriendRequest(
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId,
        CancellationToken ct = default)
    {
        var userId = claimsPrincipal.GetAuthenticatedUserId();

        var request = await context.FriendRequests.FindAsync(new object[] { friendRequestId }, ct)
            ?? throw new EntityNotFoundException("FriendRequest", friendRequestId);

        if (request.RequesteeId != userId)
            throw new AuthorizationException("You can only reject friend requests sent to you");

        context.FriendRequests.Remove(request);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} rejected friend request {RequestId}", userId, friendRequestId);
        return true;
    }

    public async Task<FriendRequest> CreateFriendRequest(
        [Service] AppDbContext context,
        FriendRequestInput input,
        CancellationToken ct = default)
    {
        var request = new FriendRequest
        {
            RequesterId = input.RequesterId,
            RequesteeId = input.RequesteeId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(3)
        };

        context.FriendRequests.Add(request);
        await context.SaveChangesAsync(ct);

        return request;
    }
}
