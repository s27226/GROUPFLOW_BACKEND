using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Projects.Entities;

namespace GROUPFLOW.Features.Projects.GraphQL;

public class ProjectEventQuery
{
    [GraphQLName("allevents")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectEvent> GetProjectEvents(AppDbContext context) => context.ProjectEvents;

    [GraphQLName("eventbyid")]
    [UseProjection]
    public ProjectEvent? GetProjectEventById(AppDbContext context, int id) => 
        context.ProjectEvents.FirstOrDefault(e => e.Id == id);

    [GraphQLName("eventsbyproject")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectEvent> GetEventsByProject(AppDbContext context, int projectId) => 
        context.ProjectEvents.Where(e => e.ProjectId == projectId);
}
