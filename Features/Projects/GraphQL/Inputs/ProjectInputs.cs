using System.ComponentModel.DataAnnotations;

namespace GROUPFLOW.Features.Projects.GraphQL.Inputs;

public record ProjectInput(
    [property: Required]
    [property: StringLength(100)]
    string Name,

    [property: Required]
    [property: StringLength(500)]
    string Description,

    [property: Url]
    string? ImageUrl,
    bool IsPublic,
    string[]? Skills,
    string[]? Interests
);

public record CreateProjectWithMembersInput(
    [property: Required]
    [property: StringLength(100)]
    string Name,

    [property: Required]
    [property: StringLength(500)]
    string Description,

    [property: Url]
    string? ImageUrl,

    bool IsPublic,

    int[] MemberUserIds,
    string[]? Skills,
    string[]? Interests
);

public record UpdateProjectInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: StringLength(100)]
    string? Name,

    [property: StringLength(500)]
    string? Description,

    [property: Url]
    string? ImageUrl,
    bool? IsPublic
);

public record SearchProjectsInput(
    string? SearchTerm,
    string[]? Skills,
    string[]? Interests
);

public record ProjectInvitationInput(
    [property: Range(1, int.MaxValue)]
    int ProjectId,

    [property: Range(1, int.MaxValue)]
    int InvitingId,

    [property: Range(1, int.MaxValue)]
    int InvitedId
);

public record UpdateProjectInvitationInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: Range(1, int.MaxValue)]
    int? ProjectId,

    [property: Range(1, int.MaxValue)]
    int? InvitingId,

    [property: Range(1, int.MaxValue)]
    int? InvitedId
);

public record ProjectEventInput(
    [property: Range(1, int.MaxValue)]
    int ProjectId,

    [property: Range(1, int.MaxValue)]
    int CreatedById,

    [property: Required]
    [property: StringLength(100)]
    string Title,

    [property: StringLength(300)]
    string? Description,

    [property: Required]
    DateTime EventDate,

    [property: StringLength(10)]
    string? Time
);

public record UpdateProjectEventInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: StringLength(100)]
    string? Title,

    [property: StringLength(300)]
    string? Description,

    DateTime? EventDate,

    [property: StringLength(10)]
    string? Time
);

public record ProjectRecommendationInput(
    [property: Range(1, int.MaxValue)]
    int UserId,

    [property: Range(1, int.MaxValue)]
    int ProjectId,

    [property: Range(1, 5)]
    int RecValue
);

public record UpdateProjectRecommendationInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: Range(1, int.MaxValue)]
    int? UserId,

    [property: Range(1, int.MaxValue)]
    int? ProjectId,

    [property: Range(1, 5)]
    int? RecValue
);
