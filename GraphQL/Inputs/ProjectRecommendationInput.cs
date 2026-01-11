using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs;

public record ProjectRecommendationInput(
    [property: Range(1, int.MaxValue)]
    int UserId,

    [property: Range(1, int.MaxValue)]
    int ProjectId,

    [property: Range(1, 5)]
    int RecValue
);
public record UpdateProjectRecommendationInput(
    [property: Range(1, int.MaxValue)]
    int Id,

    [property: Range(1, int.MaxValue)]
    int? UserId,

    [property: Range(1, int.MaxValue)]
    int? ProjectId,

    [property: Range(1, 5)]
    int? RecValue
);

