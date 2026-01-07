using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class FriendRequestQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FriendRequestQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("allfriendrequests")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<FriendRequest> GetFriendRequests()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Enumerable.Empty<FriendRequest>().AsQueryable();
        }

        // Return friend requests where the current user is the requestee (received requests)
        return _context.FriendRequests
            .Include(fr => fr.Requester)
            .Include(fr => fr.Requestee)
            .Where(fr => fr.RequesteeId == userId);
    }

}