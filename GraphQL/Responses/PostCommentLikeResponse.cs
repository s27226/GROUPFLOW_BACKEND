namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record PostCommentLikeResponse(
    int Id,
    int UserId,
    int PostCommentId,
    DateTime CreatedAt
);