namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ManageUserRoleInput(
    int UserId,
    bool IsModerator
);
