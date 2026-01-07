using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ProjectQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjectQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("allprojects")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectResponse> GetProjects() => 
        _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .Include(p => p.Collaborators)
            .Include(p => p.Likes)
            .Include(p => p.Views)
            .Where(p => p.IsPublic)
            .Select(p => ProjectResponse.FromProject(p));

    [GraphQLName("projectbyid")]
    public async Task<ProjectResponse?> GetProjectById(int id)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;
        
        var project = await _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.ImageBlob)
            .Include(p => p.BannerBlob)
            .Include(p => p.Collaborators)
                .ThenInclude(c => c.User)
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .FirstOrDefaultAsync(p => p.Id == id && 
                (p.IsPublic || (userId.HasValue && (p.OwnerId == userId.Value || 
                 p.Collaborators.Any(c => c.UserId == userId.Value)))));
        
        // Automatically record a view if user is authenticated and not the owner
        if (project != null && userId.HasValue && project.OwnerId != userId.Value)
        {
            var today = DateTime.UtcNow.Date;
            
            // Check if user already viewed today
            var existingView = await _context.ProjectViews
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
                
                _context.ProjectViews.Add(view);
                await _context.SaveChangesAsync();
            }
        }
        
        return project != null ? ProjectResponse.FromProject(project) : null;
    }

    [GraphQLName("myprojects")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectResponse> GetMyProjects()
    {
        var currentUser = _httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        int userId = int.Parse(userIdClaim);
        
        return _context.Projects
            .Include(p => p.Owner)
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .Include(p => p.Collaborators)
            .Include(p => p.Likes)
            .Include(p => p.Views)
            .Where(p => 
                p.OwnerId == userId || p.Collaborators.Any(c => c.UserId == userId))
            .Select(p => ProjectResponse.FromProject(p));
    }

    [GraphQLName("trendingprojects")]
    [UsePaging(IncludeTotalCount = true, MaxPageSize = 10)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectResponse> GetTrendingProjects()
    {
        var now = DateTime.UtcNow;
        var oneDayAgo = now.AddDays(-1);
        var oneWeekAgo = now.AddDays(-7);
        var oneMonthAgo = now.AddDays(-30);

        // Calculate trending score based on post activity with time-decay weights
        // Formula: (daily_posts * 10) + (weekly_posts * 5) + (monthly_posts * 2) + (total_posts * 0.5) + (likes * 0.3) + (views * 0.1)
        var projects = _context.Projects
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
            .Select(x => ProjectResponse.FromProject(x.Project));

        return projects;
    }

    [GraphQLName("userprojects")]
    public IQueryable<ProjectResponse> GetUserProjects(int userId)
    {
        // Return all projects the user is part of (either as owner or as member)
        return _context.Projects
            .Include(p => p.ImageBlob)
            .Include(p => p.BannerBlob)
            .Include(p => p.Owner)
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .Include(p => p.Collaborators)
            .Include(p => p.Likes)
            .Include(p => p.Views)
            .Where(p => 
                p.IsPublic && (p.OwnerId == userId || p.Collaborators.Any(up => up.UserId == userId)))
            .Select(p => ProjectResponse.FromProject(p));
    }

    [GraphQLName("projectposts")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<PostResponse> GetProjectPosts(int projectId)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        int? userId = userIdClaim != null ? int.Parse(userIdClaim) : null;
        
        var project = _context.Projects
            .Include(p => p.Collaborators)
            .FirstOrDefault(p => p.Id == projectId);
            
        if (project == null)
        {
            return Enumerable.Empty<PostResponse>().AsQueryable();
        }
        
        // Check if user is a member (owner or collaborator)
        bool isMember = userId.HasValue && 
            (project.OwnerId == userId.Value || 
             project.Collaborators.Any(c => c.UserId == userId.Value));
        
        return _context.Posts
            .Include(p => p.User)
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Where(p => p.ProjectId == projectId && 
                (p.Public || isMember)) // Show public posts OR all posts if user is a member
            .Select(p => PostResponse.FromPost(p))
            .OrderByDescending(p => p.Created);
    }

    [GraphQLName("haslikedproject")]
    public async Task<bool> HasLikedProject(int projectId)
    {
        var currentUser = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = currentUser?.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            return false; // Not authenticated
        }

        int userId = int.Parse(userIdClaim);

        return await _context.ProjectLikes
            .AnyAsync(pl => pl.ProjectId == projectId && pl.UserId == userId);
    }

    [Authorize]
    [GraphQLName("searchprojects")]
    [UseProjection]
    public async Task<List<ProjectResponse>> SearchProjects(
        ClaimsPrincipal claimsPrincipal,
        SearchProjectsInput? input = null)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return new List<ProjectResponse>();
        }

        var query = _context.Projects
            .Include(p => p.Skills)
            .Include(p => p.Interests)
            .Include(p => p.Owner)
            .Include(p => p.Collaborators)
            .Include(p => p.Likes)
            .Include(p => p.Views)
            .Where(p => p.IsPublic)
            .AsQueryable();

        // Text search by name/description
        if (!string.IsNullOrWhiteSpace(input?.SearchTerm))
        {
            var searchTerm = input.SearchTerm.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm));
        }

        // Filter by skills
        if (input?.Skills != null && input.Skills.Any())
        {
            query = query.Where(p => p.Skills.Any(s => 
                input.Skills.Contains(s.SkillName)));
        }

        // Filter by interests
        if (input?.Interests != null && input.Interests.Any())
        {
            query = query.Where(p => p.Interests.Any(i => 
                input.Interests.Contains(i.InterestName)));
        }

        return (await query.ToListAsync()).Select(p => ProjectResponse.FromProject(p)).ToList();
    }
}
