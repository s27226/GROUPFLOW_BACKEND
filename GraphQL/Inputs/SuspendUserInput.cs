using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs;

public record SuspendUserInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    [property: Required]
    DateTime SuspendedUntil
);
