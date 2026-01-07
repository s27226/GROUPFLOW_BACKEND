using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class FriendRequestMutation
{
    private readonly AppDbContext _context;

    public FriendRequestMutation(AppDbContext context)
    {
        _context = context;
    }

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
        ClaimsPrincipal claimsPrincipal,
        int requesteeId,
        CancellationToken cancellationToken)
    {
        var requesterId = GetUserId(claimsPrincipal);

        if (requesterId == requesteeId)
        {
            throw new GraphQLException("You cannot send a friend request to yourself");
        }

        var exists = await _context.FriendRequests.AnyAsync(
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

        _context.FriendRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Load navigation properties
        _context.Entry(request).Reference(r => r.Requester).Load();
        _context.Entry(request).Reference(r => r.Requestee).Load();
        
        return request;
    }

    public async Task<bool> AcceptFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(claimsPrincipal);

        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == friendRequestId, cancellationToken);

        if (request == null)
            throw new GraphQLException("Friend request not found");

        if (request.RequesteeId != userId)
            throw new GraphQLException("Not authorized");

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        _context.Friendships.AddRange(
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

        _context.FriendRequests.Remove(request);

        await _context.SaveChangesAsync(cancellationToken);
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

        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == friendRequestId, cancellationToken);

        if (request == null)
            throw new GraphQLException("Friend request not found");

        if (request.RequesteeId != userId)
            throw new GraphQLException("You can only reject friend requests sent to you");

        _context.FriendRequests.Remove(request);
        await _context.SaveChangesAsync(cancellationToken);

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
    
        var exists = await _context.FriendRequests.AnyAsync(
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
    
        _context.FriendRequests.Add(request);
        await _context.SaveChangesAsync(cancellationToken);
    
        return request;
    }

    
    
    
    

    



    public async Task<FriendRequest?> UpdateFriendRequest(
        AppDbContext context,
        UpdateFriendRequestInput input,
        CancellationToken cancellationToken)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == input.Id, cancellationToken);

        if (request == null)
            return null;
        if (input.RequesterId.HasValue) request.RequesterId = input.RequesterId.Value;
        if (input.RequesteeId.HasValue) request.RequesteeId = input.RequesteeId.Value;
        await _context.SaveChangesAsync(cancellationToken);
        return request;
    }

    public async Task<bool> DeleteFriendRequest(
        AppDbContext context,
        int id,
        CancellationToken cancellationToken)
    {
        var request = await _context.FriendRequests
            .FirstOrDefaultAsync(fr => fr.Id == id, cancellationToken);

        if (request == null)
            return false;

        _context.FriendRequests.Remove(request);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

}
