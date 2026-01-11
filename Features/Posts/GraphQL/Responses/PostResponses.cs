using GROUPFLOW.Features.Users.GraphQL.Responses;

namespace GROUPFLOW.Features.Posts.GraphQL.Responses;

/// <summary>
/// Response DTO for Post entity - isolates API layer from database model.
/// </summary>
public record PostResponse(
    int Id,
    string Title,
    string Description,
    string Content,
    string? ImageUrl,
    bool IsPublic,
    DateTime Created,
    UserResponse User,
    int LikesCount,
    int CommentsCount,
    int? ProjectId = null,
    int? SharedPostId = null
);

/// <summary>
/// Response DTO for PostLike entity.
/// </summary>
public record PostLikeResponse(
    int Id,
    int UserId,
    int PostId,
    DateTime CreatedAt
);

/// <summary>
/// Response DTO for PostComment entity.
/// </summary>
public record PostCommentResponse(
    int Id,
    string Content,
    DateTime CreatedAt,
    UserResponse User,
    int? ParentCommentId,
    int LikesCount = 0,
    int RepliesCount = 0
);

/// <summary>
/// Response DTO for PostCommentLike entity.
/// </summary>
public record PostCommentLikeResponse(
    int Id,
    int UserId,
    int CommentId,
    DateTime CreatedAt
);

/// <summary>
/// Response DTO for SavedPost entity.
/// </summary>
public record SavedPostResponse(
    int PostId,
    DateTime SavedAt,
    PostResponse Post
);
