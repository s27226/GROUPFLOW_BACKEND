using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ProjectQuery
{
    [GraphQLName("allprojects")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetProjects([Service] AppDbContext context) => 
        context.Projects.Where(p => p.IsPublic);

    [GraphQLName("projectbyid")]
    public Project? GetProjectById(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext?.User;
        var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;
        
        var project = context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
                .ThenInclude(c => c.User)
            .FirstOrDefault(p => p.Id == id && 
                (p.IsPublic || (userId.HasValue && (p.OwnerId == userId.Value || 
                 p.Collaborators.Any(c => c.UserId == userId.Value)))));
        
        return project;
    }

    [GraphQLName("myprojects")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetMyProjects(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        
        return context.Projects.Where(p => 
            p.OwnerId == userId || p.Collaborators.Any(c => c.UserId == userId));
    }

    [GraphQLName("trendingprojects")]
    [UsePaging(IncludeTotalCount = true, MaxPageSize = 10)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetTrendingProjects([Service] AppDbContext context)
    {
        return context.Projects
            .Where(p => p.IsPublic)
            .OrderByDescending(p => p.ViewCount + p.LikeCount)
            .ThenByDescending(p => p.LastUpdated);
    }

    [GraphQLName("userprojects")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Project> GetUserProjects(
        [Service] AppDbContext context,
        int userId)
    {
        // Return all projects the user is part of (either as owner or as member)
        return context.Projects.Where(p => 
            p.IsPublic && (p.OwnerId == userId || p.Collaborators.Any(up => up.UserId == userId)));
    }

    [GraphQLName("projectposts")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetProjectPosts(
        [Service] AppDbContext context,
        int projectId)
    {
        var project = context.Projects.FirstOrDefault(p => p.Id == projectId);
        if (project == null)
        {
            return Enumerable.Empty<Post>().AsQueryable();
        }
        
        return context.Posts
            .Where(p => p.ProjectId == projectId)
            .OrderByDescending(p => p.Created);
    }
}
