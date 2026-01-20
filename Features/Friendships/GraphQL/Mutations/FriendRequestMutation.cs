using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Friendships.Entities;
using GROUPFLOW.Features.Friendships.GraphQL.Inputs;

namespace GROUPFLOW.Features.Friendships.GraphQL.Mutations;

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

        // Check if request already exists from current user
        var existingRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.RequesterId == requesterId && fr.RequesteeId == requesteeId, ct);

        if (existingRequest != null)
            throw BusinessRuleException.FriendRequestPending();

        // Check if reverse request exists (other user already sent request to current user)
        var reverseRequest = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.RequesterId == requesteeId && fr.RequesteeId == requesterId, ct);

        // Check if already friends
        var alreadyFriends = await context.Friendships
            .AnyAsync(f => f.IsAccepted &&
                ((f.UserId == requesterId && f.FriendId == requesteeId) ||
                 (f.UserId == requesteeId && f.FriendId == requesterId)), ct);

        if (alreadyFriends)
            throw BusinessRuleException.AlreadyFriends();

        // If reverse request exists, accept it instead of creating a new one
        if (reverseRequest != null)
        {
            // Create bidirectional friendship, checking for existing friendships
            var existingFriendship1 = await context.Friendships
                .AnyAsync(f => f.UserId == requesteeId && f.FriendId == requesterId, ct);
            var existingFriendship2 = await context.Friendships
                .AnyAsync(f => f.UserId == requesterId && f.FriendId == requesteeId, ct);

            if (!existingFriendship1)
            {
                context.Friendships.Add(new Friendship
                {
                    UserId = requesteeId,
                    FriendId = requesterId,
                    IsAccepted = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!existingFriendship2)
            {
                context.Friendships.Add(new Friendship
                {
                    UserId = requesterId,
                    FriendId = requesteeId,
                    IsAccepted = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.FriendRequests.Remove(reverseRequest);
            await context.SaveChangesAsync(ct);

            _logger.LogInformation("User {RequesterId} auto-accepted reverse friend request from {RequesteeId}", requesterId, requesteeId);
            
            // Return a virtual FriendRequest to indicate the friendship was created
            return new FriendRequest
            {
                Id = reverseRequest.Id,
                RequesterId = requesteeId,
                RequesteeId = requesterId,
                Sent = reverseRequest.Sent,
                Expiring = reverseRequest.Expiring
            };
        }

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

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            // Check for existing friendships to prevent duplicate key violations
            var existingFriendship1 = await context.Friendships
                .AnyAsync(f => f.UserId == request.RequesterId && f.FriendId == request.RequesteeId, ct);
            var existingFriendship2 = await context.Friendships
                .AnyAsync(f => f.UserId == request.RequesteeId && f.FriendId == request.RequesterId, ct);

            // Create bidirectional friendship only if they don't exist
            if (!existingFriendship1)
            {
                context.Friendships.Add(new Friendship
                {
                    UserId = request.RequesterId,
                    FriendId = request.RequesteeId,
                    IsAccepted = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!existingFriendship2)
            {
                context.Friendships.Add(new Friendship
                {
                    UserId = request.RequesteeId,
                    FriendId = request.RequesterId,
                    IsAccepted = true,
                    CreatedAt = DateTime.UtcNow
                });
            }

            context.FriendRequests.Remove(request);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });

        _logger.LogInformation("User {UserId} accepted friend request {RequestId}", userId, friendRequestId);
        return true;
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
