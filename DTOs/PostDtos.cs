namespace NAME_WIP_BACKEND.DTOs;

/// <summary>
/// DTO for post-related operations to isolate from DB model
/// </summary>
public class PostDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int? SharedPostId { get; set; }
    public bool IsPublic { get; set; }
    public DateTime Created { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
}

public class PostCommentDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }
    public string Content { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
}

public class PostLikeDto
{
    public int UserId { get; set; }
    public int PostId { get; set; }
    public DateTime CreatedAt { get; set; }
}
