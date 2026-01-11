namespace GROUPFLOW.Features.Users.GraphQL.Responses;

/// <summary>
/// Response DTO for User entity.
/// </summary>
public record UserResponse(
    int Id,
    string Name,
    string Surname,
    string Nickname,
    string Email,
    string? ProfilePic,
    string? BannerPic,
    DateTime Joined,
    bool IsModerator = false
);

/// <summary>
/// Response DTO for UserSkill entity.
/// </summary>
public record UserSkillResponse(
    int Id,
    string SkillName,
    DateTime AddedAt
);

/// <summary>
/// Response DTO for UserInterest entity.
/// </summary>
public record UserInterestResponse(
    int Id,
    string InterestName,
    DateTime AddedAt
);
