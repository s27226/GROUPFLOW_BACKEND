using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.Models;

[Index(nameof(PostId))]
[Index(nameof(UserId))]
public class PostComment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Self-referencing relationship for replies
    public int? ParentCommentId { get; set; }
    public PostComment? ParentComment { get; set; }

    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
    public ICollection<PostComment> Replies { get; set; } = new List<PostComment>();
    public ICollection<PostCommentLike> Likes { get; set; } = new List<PostCommentLike>();
}
