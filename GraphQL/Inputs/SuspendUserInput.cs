using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record SuspendUserInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    [property: Required]
    DateTime SuspendedUntil
);
