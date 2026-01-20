using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Friendships.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Features.Friendships.GraphQL.Queries;

public class FriendRequestQuery
{
    [GraphQLName("allfriendrequests")]
    public async Task<List<FriendRequest>> GetFriendRequests(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return new List<FriendRequest>();
        }

        // Return friend requests where the current user is the requestee (received requests)
        return await context.FriendRequests
            .Include(fr => fr.Requester)
                .ThenInclude(r => r.ProfilePicBlob)
            .Include(fr => fr.Requestee)
                .ThenInclude(r => r.ProfilePicBlob)
            .Where(fr => fr.RequesteeId == userId)
            .ToListAsync();
    }

}
