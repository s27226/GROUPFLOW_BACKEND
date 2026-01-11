using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Friendships.Entities;

[Index(nameof(UserId), nameof(FriendId))]
public class Friendship
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int FriendId { get; set; }
    public User Friend { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsAccepted { get; set; } = false;
}

[Index(nameof(RequesterId), nameof(RequesteeId), IsUnique = true)]
public class FriendRequest
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int RequesteeId { get; set; }
    public DateTime Sent { get; set; }
    public DateTime Expiring { get; set; }

    public User Requester { get; set; } = null!;
    public User Requestee { get; set; } = null!;
}

[Index(nameof(RecommendedForId), nameof(RecommendedWhoId), IsUnique = true)]
public class FriendRecommendation
{
    public int Id { get; set; }
    public int RecommendedForId { get; set; }
    public int RecommendedWhoId { get; set; }
    public int RecValue { get; set; }

    public User RecommendedFor { get; set; } = null!;
    public User RecommendedWho { get; set; } = null!;
}

[Index(nameof(UserId), nameof(BlockedUserId), IsUnique = true)]
public class BlockedUser
{
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int BlockedUserId { get; set; }
    public User Blocked { get; set; } = null!;
    
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
}
