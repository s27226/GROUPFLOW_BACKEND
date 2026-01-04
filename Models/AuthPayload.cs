namespace GroupFlow_BACKEND.Models;

public record AuthPayload(
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