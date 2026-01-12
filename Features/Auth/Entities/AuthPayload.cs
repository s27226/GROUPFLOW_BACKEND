namespace GROUPFLOW.Features.Auth.Entities;

public record AuthPayload(
    int Id,
    string Name,
    string Surname,
    string Nickname,
    string Email,
    string? ProfilePic,
    bool IsModerator
)
{
    /// <summary>
    /// Alias for ProfilePic to maintain frontend compatibility
    /// </summary>
    public string? ProfilePicUrl => ProfilePic;
};
