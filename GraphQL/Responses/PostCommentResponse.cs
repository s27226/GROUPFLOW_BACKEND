using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record PostCommentResponse(
    int Id,
    string Content,
    DateTime CreatedAt,
    UserResponse User,
    int? ParentCommentId
);