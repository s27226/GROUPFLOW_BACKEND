namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ProjectEventInput(int ProjectId, int CreatedById, string Title, string? Description, DateTime EventDate, string? Time);
public record UpdateProjectEventInput(int Id, string? Title, string? Description, DateTime? EventDate, string? Time);
