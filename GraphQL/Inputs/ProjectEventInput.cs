using System.ComponentModel.DataAnnotations;
namespace GroupFlow_BACKEND.GraphQL.Inputs;

public record ProjectEventInput(
    [property: Range(1, int.MaxValue)]
    int ProjectId,

    [property: Range(1, int.MaxValue)]
    int CreatedById,

    [property: Required]
    [property: StringLength(100)]
    string Title,

    [property: StringLength(300)]
    string? Description,

    [property: Required]
    DateTime EventDate,

    [property: StringLength(10)]
    string? Time
);
public record UpdateProjectEventInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: StringLength(100)]
    string? Title,

    [property: StringLength(300)]
    string? Description,

    DateTime? EventDate,

    [property: StringLength(10)]
    string? Time
);
