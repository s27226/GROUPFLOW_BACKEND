using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class PostQuery
{
    [GraphQLName("allposts")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPosts(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        
        // Get all projects where user is owner or collaborator
        var userProjectIds = context.Projects
            .Where(p => p.OwnerId == userId || p.Collaborators.Any(c => c.UserId == userId))
            .Select(p => p.Id)
            .ToList();
        
        // Get IDs of users that the current user has blocked or has been blocked by
        var blockedUserIds = context.BlockedUsers
            .Where(bu => bu.UserId == userId || bu.BlockedUserId == userId)
            .Select(bu => bu.UserId == userId ? bu.BlockedUserId : bu.UserId)
            .ToList();
        
        // Get current user's skills and interests
        var currentUserSkills = context.UserSkills
            .Where(us => us.UserId == userId)
            .Select(us => us.SkillName.ToLower())
            .ToHashSet();
        
        var currentUserInterests = context.UserInterests
            .Where(ui => ui.UserId == userId)
            .Select(ui => ui.InterestName.ToLower())
            .ToHashSet();
        
        // Get all posts with necessary related data
        var posts = context.Posts
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Include(p => p.User)
                .ThenInclude(u => u.Skills)
            .Include(p => p.User)
                .ThenInclude(u => u.Interests)
            .Include(p => p.Project)
                .ThenInclude(pr => pr.Skills)
            .Include(p => p.Project)
                .ThenInclude(pr => pr.Interests)
            .Where(p => 
                !blockedUserIds.Contains(p.UserId) && // Exclude posts from blocked users
                (p.Public || // Public posts
                p.UserId == userId || // User's own posts
                (p.ProjectId.HasValue && userProjectIds.Contains(p.ProjectId.Value)))) // Private posts in projects user is a member of
            .AsEnumerable() // Switch to client-side evaluation for complex scoring
            .Select(p => new 
            {
                Post = p,
                RelevanceScore = CalculateRelevanceScore(p, userId, userProjectIds, currentUserSkills, currentUserInterests)
            })
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.Post.Created)
            .Select(x => x.Post)
            .AsQueryable();
        
        return posts;
    }
    
    private static int CalculateRelevanceScore(
        Post post, 
        int currentUserId, 
        List<int> userProjectIds,
        HashSet<string> userSkills,
        HashSet<string> userInterests)
    {
        int score = 0;
        
        // Check if post has recent comments (within last 24 hours)
        var hasRecentComments = post.Comments.Any(c => c.CreatedAt >= DateTime.UtcNow.AddHours(-24));
        
        // Boost own posts ONLY if they have recent comments
        if (post.UserId == currentUserId)
        {
            if (hasRecentComments)
            {
                return 10000; // High priority for own posts with recent activity
            }
            else
            {
                return 100; // Low priority for own posts without recent comments
            }
        }
        
        // Boost project posts ONLY if they have recent comments
        if (post.ProjectId.HasValue && userProjectIds.Contains(post.ProjectId.Value))
        {
            if (hasRecentComments)
            {
                return 9000; // High priority for project posts with recent activity
            }
            else
            {
                return 200; // Low priority for project posts without recent comments
            }
        }
        
        // For public posts from others, calculate relevance based on skills/interests
        // These will naturally rank higher than non-active own/project posts
        
        // 1. Calculate matching skills/interests from the post's project (if any)
        if (post.Project != null)
        {
            // Match project skills (50 points per match)
            var matchingProjectSkills = post.Project.Skills
                .Count(ps => userSkills.Contains(ps.SkillName.ToLower()));
            score += matchingProjectSkills * 50;
            
            // Match project interests (40 points per match)
            var matchingProjectInterests = post.Project.Interests
                .Count(pi => userInterests.Contains(pi.InterestName.ToLower()));
            score += matchingProjectInterests * 40;
        }
        
        // 2. Calculate matching skills/interests from the post author
        if (post.User != null)
        {
            // Match user skills (30 points per match)
            var matchingAuthorSkills = post.User.Skills
                .Count(us => userSkills.Contains(us.SkillName.ToLower()));
            score += matchingAuthorSkills * 30;
            
            // Match user interests (35 points per match)
            var matchingAuthorInterests = post.User.Interests
                .Count(ui => userInterests.Contains(ui.InterestName.ToLower()));
            score += matchingAuthorInterests * 35;
        }
        
        // Bonus for posts with recent comments from others too
        if (hasRecentComments)
        {
            score += 500; // Boost posts with recent activity
        }
        
        return score;
    }

    [GraphQLName("allpostsbyid")]
    [UseProjection]
    public Post? GetPostsById(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int id)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        
        var post = context.Posts
            .Include(p => p.Likes)
            .Include(p => p.Project)
                .ThenInclude(p => p.Collaborators)
            .FirstOrDefault(p => p.Id == id);
            
        if (post == null)
        {
            return null;
        }
        
        // Check if user can see this post
        bool canView = post.Public || // Public post
                      post.UserId == userId || // User's own post
                      (post.ProjectId.HasValue && post.Project != null && 
                       (post.Project.OwnerId == userId || // User is project owner
                        post.Project.Collaborators.Any(c => c.UserId == userId))); // User is collaborator
        
        return canView ? post : null;
    }

    [GraphQLName("isPostLikedByUser")]
    public async Task<bool> IsPostLikedByUser(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier));
        
        return await context.PostLikes
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }
}

