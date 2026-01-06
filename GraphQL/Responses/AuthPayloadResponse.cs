namespace NAME_WIP_BACKEND.GraphQL.Responses;

public record AuthPayloadResponse(
    int Id,
    string Name,
    string Surname,
    string Nickname,
    string Email,
    string? ProfilePic,
    string Token,
    string RefreshToken,
    bool IsModerator
);