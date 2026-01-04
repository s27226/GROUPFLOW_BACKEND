using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.GraphQL.Queries;

public class FriendRequestQuery
{
    [GraphQLName("allfriendrequests")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<FriendRequest> GetFriendRequests(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Enumerable.Empty<FriendRequest>().AsQueryable();
        }

        // Return friend requests where the current user is the requestee (received requests)
        return context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Requestee)
            .Where(fr => fr.RequesteeId == userId);
    }

}