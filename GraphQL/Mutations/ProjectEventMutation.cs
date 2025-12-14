using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectEventMutation
{
    public ProjectEvent CreateProjectEvent(AppDbContext context, ProjectEventInput input)
    {
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
        context.SaveChanges();
        return projectEvent;
    }

    public ProjectEvent? UpdateProjectEvent(AppDbContext context, UpdateProjectEventInput input)
    {
        var projectEvent = context.ProjectEvents.Find(input.Id);
        if (projectEvent == null) return null;
        
        if (!string.IsNullOrEmpty(input.Title)) projectEvent.Title = input.Title;
        if (input.Description != null) projectEvent.Description = input.Description;
        if (input.EventDate.HasValue) projectEvent.EventDate = input.EventDate.Value;
        if (input.Time != null) projectEvent.Time = input.Time;
        
        context.SaveChanges();
        return projectEvent;
    }

    public bool DeleteProjectEvent(AppDbContext context, int id)
    {
        var projectEvent = context.ProjectEvents.Find(id);
        if (projectEvent == null) return false;
        
        context.ProjectEvents.Remove(projectEvent);
        context.SaveChanges();
        return true;
    }
}
