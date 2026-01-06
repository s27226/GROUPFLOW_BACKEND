using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record PostLikeResponse(
    int Id,
    int UserId,
    int PostId,
    DateTime CreatedAt
);