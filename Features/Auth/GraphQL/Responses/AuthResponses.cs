namespace GROUPFLOW.Features.Auth.GraphQL.Responses;

public record AuthResponse(
    int Id,
    string Name,
    string Surname,
    string Nickname,
    string Email,
    string? ProfilePic,
    bool IsModerator
);
