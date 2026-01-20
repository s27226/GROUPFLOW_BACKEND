using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Projects.Entities;
using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Features.Projects.GraphQL.Queries;

public class ProjectRecommendationQuery
{
    [GraphQLName("allprojectrecommendations")]
    public async Task<List<ProjectRecommendation>> GetProjectRecommendations(AppDbContext context) 
    {
        return await context.ProjectRecommendations.ToListAsync();
    }
    
    [GraphQLName("projectrecommendationbyid")]
    public async Task<ProjectRecommendation?> GetProjectRecommendationById(AppDbContext context, int id) 
    {
        return await context.ProjectRecommendations.FirstOrDefaultAsync(g => g.Id == id);
    }
}
