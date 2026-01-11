using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs;

public record ResetPasswordInput(
    [property: Range(1, int.MaxValue)]
    int UserId,

    [property: Required]
    [property: MinLength(8)]
    string NewPassword
);
