using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Features.Posts.Entities;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Notifications.Entities;

[Index(nameof(UserId))]
[Index(nameof(PostId))]
public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Type { get; set; } = null!; // "POST_LIKE", "COMMENT", etc.
    public string Message { get; set; } = null!;
    
    public int? ActorUserId { get; set; }
    public User? ActorUser { get; set; }
    
    public int? PostId { get; set; }
    public Post? Post { get; set; }
    
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}
