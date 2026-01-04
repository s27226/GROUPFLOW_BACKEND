using System.ComponentModel.DataAnnotations;
namespace GroupFlow_BACKEND.GraphQL.Inputs;

public record SuspendUserInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    [property: Required]
    DateTime SuspendedUntil
);
