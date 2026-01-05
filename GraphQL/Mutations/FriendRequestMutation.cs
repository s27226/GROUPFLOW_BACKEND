using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class FriendRequestMutation
{
    private const int FriendRequestExpirationHours = 72;
    
    private static int GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        var claim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null || !int.TryParse(claim.Value, out var userId))
        {
            throw new GraphQLException("User not authenticated");
        }
        return userId;
    }
    
    
    public async Task<FriendRequest> SendFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int requesteeId,
        CancellationToken cancellationToken)
    {
        var requesterId = GetUserId(claimsPrincipal);

        if (requesterId == requesteeId)
        {
            throw new GraphQLException("You cannot send a friend request to yourself");
        }

        var exists = await context.FriendRequests.AnyAsync(
            fr => fr.RequesterId == requesterId && fr.RequesteeId == requesteeId,
            cancellationToken);

        if (exists)
        {
            throw new GraphQLException("Friend request already sent");
        }

        var request = new FriendRequest
        {
            RequesterId = requesterId,
            RequesteeId = requesteeId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(FriendRequestExpirationHours)
        };

        context.FriendRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);
        
        // Load navigation properties
        context.Entry(request).Reference(r => r.Requester).Load();
        context.Entry(request).Reference(r => r.Requestee).Load();
        
        return request;
    }

    public async Task<bool> AcceptFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(claimsPrincipal);

        var request = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == friendRequestId, cancellationToken);

        if (request == null)
            throw new GraphQLException("Friend request not found");

        if (request.RequesteeId != userId)
            throw new GraphQLException("Not authorized");

        using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

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
            });

        context.FriendRequests.Remove(request);

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    
    public async Task<bool> RejectFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(claimsPrincipal);

        var request = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == friendRequestId, cancellationToken);

        if (request == null)
            throw new GraphQLException("Friend request not found");

        if (request.RequesteeId != userId)
            throw new GraphQLException("You can only reject friend requests sent to you");

        context.FriendRequests.Remove(request);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
    public async Task<FriendRequest> CreateFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        FriendRequestInput input,
        CancellationToken cancellationToken)
    {
        var requesterId = GetUserId(claimsPrincipal);
    
        if (requesterId == input.RequesteeId)
            throw new GraphQLException("You cannot send a friend request to yourself.");
    
        var exists = await context.FriendRequests.AnyAsync(
            fr => fr.RequesterId == requesterId && fr.RequesteeId == input.RequesteeId,
            cancellationToken);
    
        if (exists)
            throw new GraphQLException("Friend request already exists");
    
        var request = new FriendRequest
        {
            RequesterId = requesterId,
            RequesteeId = input.RequesteeId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(FriendRequestExpirationHours)
        };
    
        context.FriendRequests.Add(request);
        await context.SaveChangesAsync(cancellationToken);
    
        return request;
    }

    
    
    
    

    



    public async Task<FriendRequest?> UpdateFriendRequest(
        AppDbContext context,
        UpdateFriendRequestInput input,
        CancellationToken cancellationToken)
    {
        var request = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == input.Id, cancellationToken);

        if (request == null)
            return null;
        if (input.RequesterId.HasValue) request.RequesterId = input.RequesterId.Value;
        if (input.RequesteeId.HasValue) request.RequesteeId = input.RequesteeId.Value;
        await context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<bool> DeleteFriendRequest(
        AppDbContext context,
        int id,
        CancellationToken cancellationToken)
    {
        var request = await context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == id, cancellationToken);

        if (request == null)
            return false;

        context.FriendRequests.Remove(request);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

}