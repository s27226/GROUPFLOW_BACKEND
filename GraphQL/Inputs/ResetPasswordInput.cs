namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ResetPasswordInput(
    int UserId,
    string NewPassword
);
