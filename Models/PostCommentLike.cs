using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

[Index(nameof(PostCommentId), nameof(UserId), IsUnique = true)]
public class PostCommentLike
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostCommentId { get; set; }
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public PostComment PostComment { get; set; } = null!;
}
