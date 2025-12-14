namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record ProjectRecommendationInput(int UserId, int ProjectId, int RecValue);
public record UpdateProjectRecommendationInput(int Id, int? UserId, int? ProjectId, int? RecValue);
