using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Data;
using GROUPFLOW.Models;
using GROUPFLOW.GraphQL.Inputs;

public class UsersQuery
{
    [GraphQLName("allusers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context) => context.Users;
    
    [GraphQLName("getuserbyid")]
    public User? GetUserById(AppDbContext context, int id) => 
        context.Users
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .FirstOrDefault(g => g.Id == id);
    
    [Authorize]
    [GraphQLName("me")]
    public User? GetCurrentUser(AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        
        return context.Users
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .FirstOrDefault(u => u.Id == userId);
    }
    
    [Authorize]
    [GraphQLName("searchusers")]
    [UseProjection]
    public async Task<List<UserWithRequestStatus>> SearchUsers(
        AppDbContext context, 
        ClaimsPrincipal claimsPrincipal,
        SearchUsersInput? input = null)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return new List<UserWithRequestStatus>();
        }

        // Get IDs of users that the current user has blocked or has been blocked by
        var blockedUserIds = await context.BlockedUsers
            .Where(bu => bu.UserId == currentUserId || bu.BlockedUserId == currentUserId)
            .Select(bu => bu.UserId == currentUserId ? bu.BlockedUserId : bu.UserId)
            .ToListAsync();

        var query = context.Users
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => u.Id != currentUserId && !blockedUserIds.Contains(u.Id))
            .AsQueryable();

        // Text search by name/nickname
        if (!string.IsNullOrWhiteSpace(input?.SearchTerm))
        {
            var searchTerm = input.SearchTerm.ToLower();
            query = query.Where(u => 
                u.Name.ToLower().Contains(searchTerm) ||
                u.Surname.ToLower().Contains(searchTerm) ||
                u.Nickname.ToLower().Contains(searchTerm));
        }

        // Filter by skills
        if (input?.Skills != null && input.Skills.Any())
        {
            query = query.Where(u => u.Skills.Any(s => 
                input.Skills.Contains(s.SkillName)));
        }

        // Filter by interests
        if (input?.Interests != null && input.Interests.Any())
        {
            query = query.Where(u => u.Interests.Any(i => 
                input.Interests.Contains(i.InterestName)));
        }

        var users = await query.ToListAsync();
        
        // Get all pending friend requests involving current user
        var userIds = users.Select(u => u.Id).ToList();
        var pendingRequests = await context.FriendRequests
            .Where(fr => 
                (fr.RequesterId == currentUserId && userIds.Contains(fr.RequesteeId)) ||
                (fr.RequesteeId == currentUserId && userIds.Contains(fr.RequesterId)))
            .ToListAsync();
        
        // Get friendships
        var friendships = await context.Friendships
            .Where(f => 
                (f.UserId == currentUserId && userIds.Contains(f.FriendId)) ||
                (f.FriendId == currentUserId && userIds.Contains(f.UserId)))
            .ToListAsync();

        return users.Select(u => new UserWithRequestStatus
        {
            User = u,
            HasPendingRequest = pendingRequests.Any(fr => 
                (fr.RequesterId == currentUserId && fr.RequesteeId == u.Id) ||
                (fr.RequesteeId == currentUserId && fr.RequesterId == u.Id)),
            IsFriend = friendships.Any(f => 
                (f.UserId == currentUserId && f.FriendId == u.Id) ||
                (f.FriendId == currentUserId && f.UserId == u.Id))
        }).ToList();
    }
    
    [Authorize]
    [GraphQLName("suggestedusers")]
    [UseProjection]
    public async Task<List<UserMatchScoreWithStatus>> GetSuggestedUsers(
        AppDbContext context, 
        ClaimsPrincipal claimsPrincipal,
        int limit = 10)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return new List<UserMatchScoreWithStatus>();
        }

        // Get current user's skills and interests
        var currentUser = await context.Users
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (currentUser == null)
        {
            return new List<UserMatchScoreWithStatus>();
        }

        var currentUserSkills = currentUser.Skills.Select(s => s.SkillName).ToHashSet();
        var currentUserInterests = currentUser.Interests.Select(i => i.InterestName).ToHashSet();

        // Get all users except current user and existing friends
        var existingFriendIds = await context.Friendships
            .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
            .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId)
            .ToListAsync();

        // Get IDs of users that the current user has blocked or has been blocked by
        var blockedUserIds = await context.BlockedUsers
            .Where(bu => bu.UserId == currentUserId || bu.BlockedUserId == currentUserId)
            .Select(bu => bu.UserId == currentUserId ? bu.BlockedUserId : bu.UserId)
            .ToListAsync();

        var allUsers = await context.Users
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => u.Id != currentUserId && 
                       !existingFriendIds.Contains(u.Id) && 
                       !blockedUserIds.Contains(u.Id))
            .ToListAsync();

        // Get projects the current user is part of (as owner or collaborator)
        var currentUserProjectIds = await context.UserProjects
            .Where(up => up.UserId == currentUserId)
            .Select(up => up.ProjectId)
            .Union(
                context.Projects
                    .Where(p => p.OwnerId == currentUserId)
                    .Select(p => p.Id)
            )
            .ToListAsync();

        // Get users on the same projects
        var usersOnSameProjectsAsCollaborators = await context.UserProjects
            .Where(up => currentUserProjectIds.Contains(up.ProjectId) && up.UserId != currentUserId)
            .Select(up => new { up.UserId, up.ProjectId })
            .ToListAsync();

        var usersOnSameProjectsAsOwners = await context.Projects
            .Where(p => currentUserProjectIds.Contains(p.Id) && p.OwnerId != currentUserId)
            .Select(p => new { UserId = p.OwnerId, ProjectId = p.Id })
            .ToListAsync();

        var userProjectCounts = usersOnSameProjectsAsCollaborators
            .Concat(usersOnSameProjectsAsOwners)
            .GroupBy(up => up.UserId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Get recent comment interactions (last 30 days)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        
        // Get posts by current user
        var currentUserPostIds = await context.Posts
            .Where(p => p.UserId == currentUserId)
            .Select(p => p.Id)
            .ToListAsync();

        // Get users who commented on current user's posts
        var usersWhoCommentedOnMyPosts = await context.PostComments
            .Where(pc => currentUserPostIds.Contains(pc.PostId) && 
                        pc.UserId != currentUserId &&
                        pc.CreatedAt >= thirtyDaysAgo)
            .GroupBy(pc => pc.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        // Get posts where current user commented
        var postsWhereICommented = await context.PostComments
            .Where(pc => pc.UserId == currentUserId && pc.CreatedAt >= thirtyDaysAgo)
            .Select(pc => pc.PostId)
            .Distinct()
            .ToListAsync();

        // Get other users who also commented on those posts
        var usersWhoCommentedOnSamePosts = await context.PostComments
            .Where(pc => postsWhereICommented.Contains(pc.PostId) && 
                        pc.UserId != currentUserId &&
                        pc.CreatedAt >= thirtyDaysAgo)
            .GroupBy(pc => pc.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        // Calculate match scores
        var userScores = allUsers.Select(user =>
        {
            var userSkills = user.Skills.Select(s => s.SkillName).ToHashSet();
            var userInterests = user.Interests.Select(i => i.InterestName).ToHashSet();

            // Calculate similarity using Jaccard similarity coefficient
            var commonSkills = currentUserSkills.Intersect(userSkills).Count();
            var totalSkills = currentUserSkills.Union(userSkills).Count();
            var skillSimilarity = totalSkills > 0 ? (double)commonSkills / totalSkills : 0;

            var commonInterests = currentUserInterests.Intersect(userInterests).Count();
            var totalInterests = currentUserInterests.Union(userInterests).Count();
            var interestSimilarity = totalInterests > 0 ? (double)commonInterests / totalInterests : 0;

            // Common projects score
            var commonProjects = userProjectCounts.GetValueOrDefault(user.Id, 0);
            var projectScore = Math.Min(commonProjects * 0.2, 1.0); // Cap at 1.0, each project adds 20%

            // Comment interaction score
            var commentsOnMyPosts = usersWhoCommentedOnMyPosts.GetValueOrDefault(user.Id, 0);
            var commentsOnSamePosts = usersWhoCommentedOnSamePosts.GetValueOrDefault(user.Id, 0);
            var totalInteractions = commentsOnMyPosts + commentsOnSamePosts;
            var interactionScore = Math.Min(totalInteractions * 0.1, 1.0); // Cap at 1.0, each interaction adds 10%

            // Weighted score calculation
            // - Skills: 25%
            // - Interests: 25%
            // - Common projects: 30% (strong indicator of collaboration potential)
            // - Recent interactions: 20% (shows active engagement)
            var matchScore = (
                skillSimilarity * 25 + 
                interestSimilarity * 25 + 
                projectScore * 30 + 
                interactionScore * 20
            );

            return new UserMatchScore
            {
                User = user,
                MatchScore = matchScore,
                CommonSkills = commonSkills,
                CommonInterests = commonInterests,
                CommonProjects = commonProjects,
                RecentInteractions = totalInteractions
            };
        })
        .Where(s => s.MatchScore > 0) // Only users with at least some match
        .OrderByDescending(s => s.MatchScore)
        .Take(limit)
        .ToList();
        
        // Get pending friend requests for these users
        var userIds = userScores.Select(s => s.User.Id).ToList();
        var pendingRequests = await context.FriendRequests
            .Where(fr => 
                (fr.RequesterId == currentUserId && userIds.Contains(fr.RequesteeId)) ||
                (fr.RequesteeId == currentUserId && userIds.Contains(fr.RequesterId)))
            .ToListAsync();

        return userScores.Select(s => new UserMatchScoreWithStatus
        {
            User = s.User,
            MatchScore = s.MatchScore,
            CommonSkills = s.CommonSkills,
            CommonInterests = s.CommonInterests,
            CommonProjects = s.CommonProjects,
            RecentInteractions = s.RecentInteractions,
            HasPendingRequest = pendingRequests.Any(fr => 
                (fr.RequesterId == currentUserId && fr.RequesteeId == s.User.Id) ||
                (fr.RequesteeId == currentUserId && fr.RequesterId == s.User.Id))
        }).ToList();
    }
}

public class UserMatchScore
{
    public User User { get; set; } = null!;
    public double MatchScore { get; set; }
    public int CommonSkills { get; set; }
    public int CommonInterests { get; set; }
    public int CommonProjects { get; set; }
    public int RecentInteractions { get; set; }
}

public class UserMatchScoreWithStatus
{
    public User User { get; set; } = null!;
    public double MatchScore { get; set; }
    public int CommonSkills { get; set; }
    public int CommonInterests { get; set; }
    public int CommonProjects { get; set; }
    public int RecentInteractions { get; set; }
    public bool HasPendingRequest { get; set; }
}

public class UserWithRequestStatus
{
    public User User { get; set; } = null!;
    public bool HasPendingRequest { get; set; }
    public bool IsFriend { get; set; }
}
