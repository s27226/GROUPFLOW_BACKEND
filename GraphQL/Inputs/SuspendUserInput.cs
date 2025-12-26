namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record SuspendUserInput(
    int UserId,
    DateTime SuspendedUntil
);
