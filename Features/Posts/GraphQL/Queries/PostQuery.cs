using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Posts.Entities;

namespace GROUPFLOW.Features.Posts.GraphQL.Queries;

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
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var userProjectIds = context.Projects
            .Where(p => p.OwnerId == userId || p.Collaborators.Any(c => c.UserId == userId))
            .Select(p => p.Id)
            .ToList();
        
        var blockedUserIds = context.BlockedUsers
            .Where(bu => bu.UserId == userId || bu.BlockedUserId == userId)
            .Select(bu => bu.UserId == userId ? bu.BlockedUserId : bu.UserId)
            .ToList();
        
        var currentUserSkills = context.UserSkills
            .Where(us => us.UserId == userId)
            .Select(us => us.SkillName.ToLower())
            .ToHashSet();
        
        var currentUserInterests = context.UserInterests
            .Where(ui => ui.UserId == userId)
            .Select(ui => ui.InterestName.ToLower())
            .ToHashSet();
        
        var posts = context.Posts
            .Include(p => p.Likes)
            .Include(p => p.Comments)
            .Include(p => p.User)
                .ThenInclude(u => u.Skills)
            .Include(p => p.User)
                .ThenInclude(u => u.Interests)
            .Include(p => p.Project)
                .ThenInclude(pr => pr!.Skills)
            .Include(p => p.Project)
                .ThenInclude(pr => pr!.Interests)
            .Where(p => 
                !blockedUserIds.Contains(p.UserId) &&
                (p.Public ||
                p.UserId == userId ||
                (p.ProjectId.HasValue && userProjectIds.Contains(p.ProjectId.Value))))
            .AsEnumerable()
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
        
        var hasRecentComments = post.Comments.Any(c => c.CreatedAt >= DateTime.UtcNow.AddHours(-24));
        
        if (post.UserId == currentUserId)
        {
            return hasRecentComments ? 10000 : 100;
        }
        
        if (post.ProjectId.HasValue && userProjectIds.Contains(post.ProjectId.Value))
        {
            return hasRecentComments ? 9000 : 200;
        }
        
        if (post.Project != null)
        {
            var matchingProjectSkills = post.Project.Skills
                .Count(ps => userSkills.Contains(ps.SkillName.ToLower()));
            score += matchingProjectSkills * 50;
            
            var matchingProjectInterests = post.Project.Interests
                .Count(pi => userInterests.Contains(pi.InterestName.ToLower()));
            score += matchingProjectInterests * 40;
        }
        
        if (post.User != null)
        {
            var matchingAuthorSkills = post.User.Skills
                .Count(us => userSkills.Contains(us.SkillName.ToLower()));
            score += matchingAuthorSkills * 30;
            
            var matchingAuthorInterests = post.User.Interests
                .Count(ui => userInterests.Contains(ui.InterestName.ToLower()));
            score += matchingAuthorInterests * 35;
        }
        
        if (hasRecentComments)
        {
            score += 500;
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
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var post = context.Posts
            .Include(p => p.Likes)
            .Include(p => p.Project)
                .ThenInclude(p => p!.Collaborators)
            .FirstOrDefault(p => p.Id == id);
            
        if (post == null)
        {
            return null;
        }
        
        bool canView = post.Public ||
                      post.UserId == userId ||
                      (post.ProjectId.HasValue && post.Project != null && 
                       (post.Project.OwnerId == userId ||
                        post.Project.Collaborators.Any(c => c.UserId == userId)));
        
        return canView ? post : null;
    }

    [GraphQLName("isPostLikedByUser")]
    public async Task<bool> IsPostLikedByUser(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int postId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        return await context.PostLikes
            .AnyAsync(pl => pl.PostId == postId && pl.UserId == userId);
    }
}
