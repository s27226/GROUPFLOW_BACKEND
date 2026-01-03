using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ResetPasswordInput(
    [property: Range(1, int.MaxValue)]
    int UserId,

    [property: Required]
    [property: MinLength(8)]
    string NewPassword
);
