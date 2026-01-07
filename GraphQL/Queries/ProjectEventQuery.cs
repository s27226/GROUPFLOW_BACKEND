using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ProjectEventQuery
{
    private readonly AppDbContext _context;

    public ProjectEventQuery(AppDbContext context)
    {
        _context = context;
    }

    [GraphQLName("allevents")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectEvent> GetProjectEvents() => _context.ProjectEvents;

    [GraphQLName("eventbyid")]
    [UseProjection]
    public ProjectEvent? GetProjectEventById(int id) => 
        _context.ProjectEvents.FirstOrDefault(e => e.Id == id);

    [GraphQLName("eventsbyproject")]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectEvent> GetEventsByProject(int projectId) => 
        _context.ProjectEvents.Where(e => e.ProjectId == projectId);
}
