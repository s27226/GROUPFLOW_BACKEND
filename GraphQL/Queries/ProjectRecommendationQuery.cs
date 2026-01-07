using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ProjectRecommendationQuery
{
    private readonly AppDbContext _context;

    public ProjectRecommendationQuery(AppDbContext context)
    {
        _context = context;
    }

    [GraphQLName("allprojectrecommendations")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectRecommendation> GetProjectRecommendations() => _context.ProjectRecommendations;
    
    [GraphQLName("projectrecommendationbyid")]
    [UseProjection]
    public ProjectRecommendation? GetProjectRecommendationById(int id) => _context.ProjectRecommendations.FirstOrDefault(g => g.Id == id);
}
