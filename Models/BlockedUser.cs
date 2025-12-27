namespace NAME_WIP_BACKEND.Models
{
    public class BlockedUser
    {
        public int Id { get; set; }
        
        // The user who is doing the blocking
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        // The user who is being blocked
        public int BlockedUserId { get; set; }
        public User Blocked { get; set; } = null!;
        
        public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
    }
}
