using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

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