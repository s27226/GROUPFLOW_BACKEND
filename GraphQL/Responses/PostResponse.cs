using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record PostResponse(
    int Id,
    string Title,
    string Description,
    string Content,
    string? ImageUrl,
    bool Public,
    DateTime Created,
    UserResponse User,
    int LikesCount,
    int CommentsCount
);