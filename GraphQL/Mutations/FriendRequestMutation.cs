using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.GraphQL.Inputs;
using GroupFlow_BACKEND.Models;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

public class FriendRequestMutation
{
    public FriendRequest? SendFriendRequest(
        AppDbContext context, 
        ClaimsPrincipal claimsPrincipal,
        int requesteeId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int requesterId))
        {
            throw new GraphQLException("User not authenticated");
        }

        // Check if request already exists
        var existingRequest = context.FriendRequests
            .FirstOrDefault(fr => fr.RequesterId == requesterId && fr.RequesteeId == requesteeId);
        
        if (existingRequest != null)
        {
            throw new GraphQLException("Friend request already sent");
        }

        var request = new FriendRequest
        {
            RequesterId = requesterId,
            RequesteeId = requesteeId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(72)
        };
        
        context.FriendRequests.Add(request);
        context.SaveChanges();
        
        // Load navigation properties
        context.Entry(request).Reference(r => r.Requester).Load();
        context.Entry(request).Reference(r => r.Requestee).Load();
        
        return request;
    }

    public bool AcceptFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new GraphQLException("User not authenticated");
        }

        var request = context.FriendRequests.Find(friendRequestId);
        if (request == null)
        {
            throw new GraphQLException("Friend request not found");
        }

        // Verify that the current user is the requestee
        if (request.RequesteeId != userId)
        {
            throw new GraphQLException("You can only accept friend requests sent to you");
        }

        // Create friendship (bidirectional)
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

        context.Friendships.AddRange(friendship1, friendship2);
        
        // Remove the friend request
        context.FriendRequests.Remove(request);
        context.SaveChanges();

        return true;
    }

    public bool RejectFriendRequest(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        int friendRequestId)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new GraphQLException("User not authenticated");
        }

        var request = context.FriendRequests.Find(friendRequestId);
        if (request == null)
        {
            throw new GraphQLException("Friend request not found");
        }

        // Verify that the current user is the requestee
        if (request.RequesteeId != userId)
        {
            throw new GraphQLException("You can only reject friend requests sent to you");
        }

        // Simply remove the friend request
        context.FriendRequests.Remove(request);
        context.SaveChanges();

        return true;
    }

    public FriendRequest CreateFriendRequest(AppDbContext context, FriendRequestInput input)
    {
        var request = new FriendRequest
        {
            RequesterId = input.RequesterId,
            RequesteeId = input.RequesteeId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.Now.AddHours(3)
        };
        context.FriendRequests.Add(request);
        context.SaveChanges();
        return request;
    }

    public FriendRequest? UpdateFriendRequest(AppDbContext context, UpdateFriendRequestInput input)
    {
        var request = context.FriendRequests.Find(input.Id);
        if (request == null) return null;
        if (input.RequesterId.HasValue) request.RequesterId = input.RequesterId.Value;
        if (input.RequesteeId.HasValue) request.RequesteeId = input.RequesteeId.Value;
        context.SaveChanges();
        return request;
    }

    public bool DeleteFriendRequest(AppDbContext context, int id)
    {
        var request = context.FriendRequests.Find(id);
        if (request == null) return false;
        context.FriendRequests.Remove(request);
        context.SaveChanges();
        return true;
    }
}