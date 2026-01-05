using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.Services;

public class ProjectEventService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectEventService> _logger;

    public ProjectEventService(AppDbContext context, ILogger<ProjectEventService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<ProjectEvent> CreateProjectEvent(ClaimsPrincipal user, ProjectEventInput input)
    {
        int userId = GetUserId(user);

        var project = await _context.Projects.FindAsync(input.ProjectId);
        if (project == null)
            throw new GraphQLException("Project not found");

        var projectEvent = new ProjectEvent
        {
            ProjectId = input.ProjectId,
            CreatedById = userId,
            Title = input.Title,
            Description = input.Description,
            EventDate = input.EventDate,
            Time = input.Time,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectEvents.Add(projectEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} created event {EventId} in project {ProjectId}", 
            userId, projectEvent.Id, input.ProjectId);

        return projectEvent;
    }

    public async Task<ProjectEvent> UpdateProjectEvent(ClaimsPrincipal user, UpdateProjectEventInput input)
    {
        int userId = GetUserId(user);

        var projectEvent = await _context.ProjectEvents
            .Include(pe => pe.Project)
            .FirstOrDefaultAsync(pe => pe.Id == input.Id);

        if (projectEvent == null)
            throw new GraphQLException("Event not found");

        if (projectEvent.CreatedById != userId && projectEvent.Project.OwnerId != userId)
            throw new GraphQLException("You don't have permission to update this event");

        if (!string.IsNullOrWhiteSpace(input.Title))
            projectEvent.Title = input.Title;

        if (input.Description != null)
            projectEvent.Description = input.Description;

        if (input.EventDate.HasValue)
            projectEvent.EventDate = input.EventDate.Value;

        if (input.Time != null)
            projectEvent.Time = input.Time;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated event {EventId} in project {ProjectId}", 
            userId, projectEvent.Id, projectEvent.ProjectId);

        return projectEvent;
    }

    public async Task<bool> DeleteProjectEvent(ClaimsPrincipal user, int eventId)
    {
        int userId = GetUserId(user);

        var projectEvent = await _context.ProjectEvents
            .Include(pe => pe.Project)
            .FirstOrDefaultAsync(pe => pe.Id == eventId);

        if (projectEvent == null)
            throw new GraphQLException("Event not found");

        if (projectEvent.CreatedById != userId && projectEvent.Project.OwnerId != userId)
            throw new GraphQLException("You don't have permission to delete this event");

        _context.ProjectEvents.Remove(projectEvent);
        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} deleted event {EventId} from project {ProjectId}", 
            userId, eventId, projectEvent.ProjectId);

        return true;
    }
}
