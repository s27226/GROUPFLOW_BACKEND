using GROUPFLOW.Data;
using GROUPFLOW.Models;

namespace GROUPFLOW.GraphQL.Queries;

public class ProjectRecommendationQuery
{
    [GraphQLName("allprojectrecommendations")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectRecommendation> GetProjectRecommendations(AppDbContext context) => context.ProjectRecommendations;
    
    [GraphQLName("projectrecommendationbyid")]
    [UseProjection]
    public ProjectRecommendation? GetProjectRecommendationById(AppDbContext context, int id) => context.ProjectRecommendations.FirstOrDefault(g => g.Id == id);
}
