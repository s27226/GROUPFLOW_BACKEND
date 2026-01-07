using System.Security.Claims;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class FriendshipQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FriendshipQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("myfriends")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetMyFriends()
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return _context.Users
            .Where(u => _context.Friendships
                .Any(f => (f.UserId == userId && f.FriendId == u.Id && f.IsAccepted) ||
                         (f.FriendId == userId && f.UserId == u.Id && f.IsAccepted)));
    }

    [GraphQLName("friendshipstatus")]
    public string GetFriendshipStatus(int friendId)
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var friendship = _context.Friendships
            .FirstOrDefault(f => 
                (f.UserId == userId && f.FriendId == friendId) ||
                (f.FriendId == userId && f.UserId == friendId));

        if (friendship == null) return "none";
        if (friendship.IsAccepted) return "friends";
        if (friendship.UserId == userId) return "pending_sent";
        return "pending_received";
    }
}