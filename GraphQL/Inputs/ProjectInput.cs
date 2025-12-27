namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ProjectInput(
    string Name,
    string Description,
    string? ImageUrl,
    bool IsPublic,
    string[]? Skills,
    string[]? Interests
);

public record CreateProjectWithMembersInput(
    string Name,
    string Description,
    string? ImageUrl,
    bool IsPublic,
    int[] MemberUserIds,
    string[]? Skills,
    string[]? Interests
);

public record UpdateProjectInput(
    int Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    bool? IsPublic
);

public record SearchProjectsInput(
    string? SearchTerm,
    string[]? Skills,
    string[]? Interests
);
