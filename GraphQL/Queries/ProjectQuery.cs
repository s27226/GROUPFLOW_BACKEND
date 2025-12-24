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
    public async Task<Project?> GetProjectById(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext?.User;
        var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;
        
        var project = await context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
                .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(p => p.Id == id && 
                (p.IsPublic || (userId.HasValue && (p.OwnerId == userId.Value || 
                 p.Collaborators.Any(c => c.UserId == userId.Value)))));
        
        // Automatically record a view if user is authenticated and not the owner
        if (project != null && userId.HasValue && project.OwnerId != userId.Value)
        {
            var today = DateTime.UtcNow.Date;
            
            // Check if user already viewed today
            var existingView = await context.ProjectViews
                .FirstOrDefaultAsync(pv => pv.ProjectId == id && 
                                          pv.UserId == userId.Value && 
                                          pv.ViewDate == today);
            
            if (existingView == null)
            {
                // Create new view record
                var view = new ProjectView
                {
                    ProjectId = id,
                    UserId = userId.Value,
                    ViewDate = today,
                    Created = DateTime.UtcNow
                };
                
                context.ProjectViews.Add(view);
                await context.SaveChangesAsync();
            }
        }
        
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
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        int userId = int.Parse(userIdClaim);
        
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
        var now = DateTime.UtcNow;
        var oneDayAgo = now.AddDays(-1);
        var oneWeekAgo = now.AddDays(-7);
        var oneMonthAgo = now.AddDays(-30);

        // Calculate trending score based on post activity with time-decay weights
        // Formula: (daily_posts * 10) + (weekly_posts * 5) + (monthly_posts * 2) + (total_posts * 0.5) + (likes * 0.3) + (views * 0.1)
        var projects = context.Projects
            .Where(p => p.IsPublic)
            .Select(p => new
            {
                Project = p,
                DailyPosts = p.Posts.Count(post => post.Created >= oneDayAgo),
                WeeklyPosts = p.Posts.Count(post => post.Created >= oneWeekAgo && post.Created < oneDayAgo),
                MonthlyPosts = p.Posts.Count(post => post.Created >= oneMonthAgo && post.Created < oneWeekAgo),
                TotalPosts = p.Posts.Count,
                // Count actual likes and views from the database tables
                ActualLikes = p.Likes.Count,
                ActualViews = p.Views.Count,
                TrendingScore = 
                    (p.Posts.Count(post => post.Created >= oneDayAgo) * 10.0) +
                    (p.Posts.Count(post => post.Created >= oneWeekAgo && post.Created < oneDayAgo) * 5.0) +
                    (p.Posts.Count(post => post.Created >= oneMonthAgo && post.Created < oneWeekAgo) * 2.0) +
                    (p.Posts.Count * 0.5) +
                    (p.Likes.Count * 0.3) +
                    (p.Views.Count * 0.1)
            })
            .OrderByDescending(x => x.TrendingScore)
            .ThenByDescending(x => x.Project.LastUpdated)
            .Select(x => x.Project);

        return projects;
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

    [GraphQLName("haslikedproject")]
    public async Task<bool> HasLikedProject(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int projectId)
    {
        var currentUser = httpContextAccessor.HttpContext?.User;
        var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            return false; // Not authenticated
        }

        int userId = int.Parse(userIdClaim);

        return await context.ProjectLikes
            .AnyAsync(pl => pl.ProjectId == projectId && pl.UserId == userId);
    }
}
