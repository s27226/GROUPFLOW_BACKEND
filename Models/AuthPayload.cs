namespace GROUPFLOW.Models;

public record AuthPayload(
    int Id,
    string Name,
    string Surname,
    string Nickname,
    string Email,
    string? ProfilePic,
    bool IsModerator
    );