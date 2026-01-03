using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ManageUserRoleInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    bool IsModerator
);
