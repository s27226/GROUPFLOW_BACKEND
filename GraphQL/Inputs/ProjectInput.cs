namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ProjectInput(
    string Name,
    string Description,
    string? ImageUrl,
    bool IsPublic
);

public record UpdateProjectInput(
    int Id,
    string? Name,
    string? Description,
    string? ImageUrl,
    bool? IsPublic
);
