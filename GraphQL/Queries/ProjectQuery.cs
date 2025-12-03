using System.Security.Claims;
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
    [UseProjection]
    public Project? GetProjectById(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        
        return context.Projects
            .FirstOrDefault(p => p.Id == id && 
                (p.IsPublic || p.OwnerId == userId || 
                 p.Collaborators.Any(c => c.UserId == userId)));
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
        return context.Projects.Where(p => 
            p.OwnerId == userId && p.IsPublic);
    }
}