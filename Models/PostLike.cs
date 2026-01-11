using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models;
[Index(nameof(UserId), nameof(PostId), IsUnique = true)]
public class PostLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}
