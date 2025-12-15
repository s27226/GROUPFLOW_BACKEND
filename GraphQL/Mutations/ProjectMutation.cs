using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectMutation
{
    public Project? CreateProject(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        ProjectInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var project = new Project
        {
            Name = input.Name,
            Description = input.Description,
            ImageUrl = input.ImageUrl,
            IsPublic = input.IsPublic,
            OwnerId = userId,
            Created = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow,
            ViewCount = 0,
            LikeCount = 0
        };

        context.Projects.Add(project);
        context.SaveChanges();
        
        // Load the owner to return complete object
        context.Entry(project).Reference(p => p.Owner).Load();
        
        return project;
    }

    public Project? UpdateProject(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UpdateProjectInput input)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        var project = context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
            .FirstOrDefault(p => p.Id == input.Id);

        if (project == null)
        {
            return null;
        }

        // Check if user is the owner
        if (project.OwnerId != userId)
        {
            throw new GraphQLException("You don't have permission to edit this project");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(input.Name))
        {
            project.Name = input.Name;
        }
        
        if (!string.IsNullOrEmpty(input.Description))
        {
            project.Description = input.Description;
        }
        
        if (input.ImageUrl != null)
        {
            project.ImageUrl = input.ImageUrl;
        }
        
        if (input.IsPublic.HasValue)
        {
            project.IsPublic = input.IsPublic.Value;
        }

        project.LastUpdated = DateTime.UtcNow;

        context.SaveChanges();
        return project;
    }

    public bool DeleteProject(
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

        var project = context.Projects.Find(id);
        
        if (project == null)
        {
            return false;
        }

        // Check if user is the owner
        if (project.OwnerId != userId)
        {
            throw new GraphQLException("You don't have permission to delete this project");
        }

        context.Projects.Remove(project);
        context.SaveChanges();
        return true;
    }
}
