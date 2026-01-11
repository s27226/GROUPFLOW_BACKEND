using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models
{
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
}