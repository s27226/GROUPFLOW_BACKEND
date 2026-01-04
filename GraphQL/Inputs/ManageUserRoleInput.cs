using System.ComponentModel.DataAnnotations;
namespace GroupFlow_BACKEND.GraphQL.Inputs;

public record ManageUserRoleInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    bool IsModerator
);
