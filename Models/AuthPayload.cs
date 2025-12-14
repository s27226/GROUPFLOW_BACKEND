namespace NAME_WIP_BACKEND.Models;

public record AuthPayload(
    int Id,
    string Name,
    string Email,
    string Token,
    string RefreshToken
    );