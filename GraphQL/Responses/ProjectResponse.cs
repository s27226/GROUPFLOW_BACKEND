namespace GROUPFLOW.GraphQL.Responses;

/// <summary>
/// Response DTO for Project entity - isolates API layer from database model.
/// </summary>
public record ProjectResponse(
    int Id,
    string Name,
    string Description,
    string? Image,
    string? Banner,
    bool IsPublic,
    DateTime Created,
    DateTime LastUpdated,
    UserResponse Owner,
    int LikesCount = 0,
    int ViewsCount = 0,
    int MembersCount = 0,
    IEnumerable<string>? Skills = null,
    IEnumerable<string>? Interests = null
);

/// <summary>
/// Response DTO for project membership.
/// </summary>
public record ProjectMemberResponse(
    int UserId,
    int ProjectId,
    string Role,
    UserResponse User
);

/// <summary>
/// Response DTO for project invitation.
/// </summary>
public record ProjectInvitationResponse(
    int Id,
    int ProjectId,
    string ProjectName,
    int InvitingUserId,
    string InvitingUserName,
    int InvitedUserId,
    DateTime Sent,
    DateTime Expiring
);

/// <summary>
/// Response DTO for project event.
/// </summary>
public record ProjectEventResponse(
    int Id,
    int ProjectId,
    string Title,
    string Description,
    DateTime EventDate,
    string Time,
    DateTime CreatedAt,
    UserResponse CreatedBy
);
