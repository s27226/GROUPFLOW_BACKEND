using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Features.Blobs.Entities;
using GROUPFLOW.Features.Projects.Entities;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Posts.Entities;

[Index(nameof(UserId))]
[Index(nameof(ProjectId))]
[Index(nameof(Created))]
public class Post
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? ImageUrl { get; set; }
    
    // Blob storage reference
    public int? ImageBlobId { get; set; }
    public BlobFile? ImageBlob { get; set; }

    public string Content { get; set; } = null!;

    public bool Public { get; set; } = true;

    public DateTime Created { get; set; }

    // Self-referencing relationship for shared posts
    public int? SharedPostId { get; set; }
    public Post? SharedPost { get; set; }

    public ICollection<SavedPost> SavedBy { get; set; } = new List<SavedPost>();
    public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
    
    [GraphQLIgnore]
    public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
}

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

[Index(nameof(PostId))]
public class PostReport
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    
    public int ReportedBy { get; set; }
    public User ReportedByUser { get; set; } = null!;
    
    public string Reason { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsResolved { get; set; } = false;
}

[Index(nameof(UserId), nameof(PostId), IsUnique = true)]
public class SavedPost
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
