namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ChatInput(
    int ProjectId
);

public record UpdateChatInput(
    int Id,
    int? NewProjectId
);