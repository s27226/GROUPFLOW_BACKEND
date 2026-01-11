using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for project event operations.
/// </summary>
public class ProjectEventMutation
{
    private readonly ILogger<ProjectEventMutation> _logger;

    public ProjectEventMutation(ILogger<ProjectEventMutation> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectEvent> CreateProjectEvent(
        [Service] AppDbContext context,
        ProjectEventInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        var projectEvent = new ProjectEvent
        {
            ProjectId = input.ProjectId,
            CreatedById = input.CreatedById,
            Title = input.Title,
            Description = input.Description,
            EventDate = input.EventDate,
            Time = input.Time,
            CreatedAt = DateTime.UtcNow
        };

        context.ProjectEvents.Add(projectEvent);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Created project event {EventId} for project {ProjectId}", projectEvent.Id, input.ProjectId);
        return projectEvent;
    }

    public async Task<ProjectEvent?> UpdateProjectEvent(
        [Service] AppDbContext context,
        UpdateProjectEventInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        var projectEvent = await context.ProjectEvents.FindAsync(new object[] { input.Id }, ct);
        if (projectEvent == null) return null;

        if (!string.IsNullOrEmpty(input.Title)) projectEvent.Title = input.Title;
        if (input.Description != null) projectEvent.Description = input.Description;
        if (input.EventDate.HasValue) projectEvent.EventDate = input.EventDate.Value;
        if (input.Time != null) projectEvent.Time = input.Time;

        await context.SaveChangesAsync(ct);
        return projectEvent;
    }

    public async Task<bool> DeleteProjectEvent(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var projectEvent = await context.ProjectEvents
            .Include(pe => pe.Project)
            .FirstOrDefaultAsync(pe => pe.Id == id, ct)
            ?? throw new EntityNotFoundException("ProjectEvent", id);

        // Check if user is the event creator or project owner
        if (projectEvent.CreatedById != userId && projectEvent.Project.OwnerId != userId)
            throw new AuthorizationException("You don't have permission to delete this event");

        context.ProjectEvents.Remove(projectEvent);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} deleted project event {EventId}", userId, id);
        return true;
    }
}
