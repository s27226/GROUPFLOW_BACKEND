namespace NAME_WIP_BACKEND.GraphQL.Responses;

/// <summary>
/// Response DTO for User entity - isolates API layer from database model.
/// </summary>
public record UserResponse
{
    public int Id { get; init; }
    public required string Name { get; init; }
    public required string Surname { get; init; }
    public required string Nickname { get; init; }
    public required string Email { get; init; }
    public string? ProfilePic { get; init; }
    public string? BannerPic { get; init; }
    public DateTime Joined { get; init; }
    public bool IsModerator { get; init; }
    public bool IsBanned { get; init; }
}
