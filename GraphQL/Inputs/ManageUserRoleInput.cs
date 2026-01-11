using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs;

public record ManageUserRoleInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    bool IsModerator
);
