using System.Security.Claims;
using GroupFlow_BACKEND.Data;
using GroupFlow_BACKEND.GraphQL.Inputs;
using GroupFlow_BACKEND.Models;
using Microsoft.EntityFrameworkCore;
using HotChocolate;

namespace GroupFlow_BACKEND.GraphQL.Mutations;

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

    public bool DeleteProjectEvent(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var projectEvent = context.ProjectEvents
            .Include(pe => pe.Project)
            .FirstOrDefault(pe => pe.Id == id);
            
        if (projectEvent == null)
        {
            throw new GraphQLException("Event not found");
        }
        
        // Check if user is the event creator or project owner
        if (projectEvent.CreatedById != userId && projectEvent.Project.OwnerId != userId)
        {
            throw new GraphQLException("You don't have permission to delete this event");
        }
        
        context.ProjectEvents.Remove(projectEvent);
        context.SaveChanges();
        return true;
    }
}
