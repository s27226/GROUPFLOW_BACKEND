using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models
{
    [Index(nameof(UserId), nameof(PostId), IsUnique = true)]
    public class SavedPost
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
