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
        
        return context.Posts
            .Include(p => p.Likes)
            .Where(p => 
                !blockedUserIds.Contains(p.UserId) && // Exclude posts from blocked users
                (p.Public || // Public posts
                p.UserId == userId || // User's own posts
                (p.ProjectId.HasValue && userProjectIds.Contains(p.ProjectId.Value)))); // Private posts in projects user is a member of
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

