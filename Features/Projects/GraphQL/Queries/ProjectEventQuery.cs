using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Projects.Entities;
using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Features.Projects.GraphQL.Queries;

public class ProjectEventQuery
{
    [GraphQLName("allevents")]
    public async Task<List<ProjectEvent>> GetProjectEvents(AppDbContext context) 
    {
        return await context.ProjectEvents.ToListAsync();
    }

    [GraphQLName("eventbyid")]
    public async Task<ProjectEvent?> GetProjectEventById(AppDbContext context, int id) 
    {
        return await context.ProjectEvents.FirstOrDefaultAsync(e => e.Id == id);
    }

    [GraphQLName("eventsbyproject")]
    public async Task<List<ProjectEvent>> GetEventsByProject(AppDbContext context, int projectId) 
    {
        return await context.ProjectEvents
            .Where(e => e.ProjectId == projectId)
            .ToListAsync();
    }
}
