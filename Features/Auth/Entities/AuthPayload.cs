namespace GROUPFLOW.Features.Auth.Entities;

public record AuthPayload(
    int Id,
    string Name,
    string Surname,
    string Nickname,
    string Email,
    string? ProfilePic,
    bool IsModerator
);
