using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.Models;

namespace GroupFlow_BACKEND.GraphQL.Queries;

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
