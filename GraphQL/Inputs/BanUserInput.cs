namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record BanUserInput(
    int UserId,
    string Reason,
    DateTime? ExpiresAt
);
