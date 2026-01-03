using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.GraphQL.Inputs;

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

    [property: MinLength(1)]
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
