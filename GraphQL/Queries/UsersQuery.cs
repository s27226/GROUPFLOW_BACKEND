using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.GraphQL.Responses;

public class UsersQuery
{
    private readonly AppDbContext _context;

    public UsersQuery(AppDbContext context)
    {
        _context = context;
    }

    [GraphQLName("allusers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<UserResponse> GetUsers() =>
        _context.Users
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Select(u => UserResponse.FromUser(u));
    
    [GraphQLName("getuserbyid")]
    public UserResponse? GetUserById(int id) => 
        _context.Users
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(g => g.Id == id)
            .Select(u => UserResponse.FromUser(u))
            .FirstOrDefault();
    
    [Authorize]
    [GraphQLName("me")]
    public UserResponse? GetCurrentUser(ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        
        return _context.Users
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => u.Id == userId)
            .Select(u => UserResponse.FromUser(u))
            .FirstOrDefault();
    }
    
    [Authorize]
    [GraphQLName("searchusers")]
    [UseProjection]
    public async Task<List<UserWithRequestStatusResponse>> SearchUsers(
        ClaimsPrincipal claimsPrincipal,
        SearchUsersInput? input = null)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return new List<UserWithRequestStatusResponse>();
        }

        // Get IDs of users that the current user has blocked or has been blocked by
        var blockedUserIds = await _context.BlockedUsers
            .Where(bu => bu.UserId == currentUserId || bu.BlockedUserId == currentUserId)
            .Select(bu => bu.UserId == currentUserId ? bu.BlockedUserId : bu.UserId)
            .ToListAsync();

        var query = _context.Users
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
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
        var pendingRequests = await _context.FriendRequests
            .Where(fr => 
                (fr.RequesterId == currentUserId && userIds.Contains(fr.RequesteeId)) ||
                (fr.RequesteeId == currentUserId && userIds.Contains(fr.RequesterId)))
            .ToListAsync();
        
        // Get friendships
        var friendships = await _context.Friendships
            .Where(f => 
                (f.UserId == currentUserId && userIds.Contains(f.FriendId)) ||
                (f.FriendId == currentUserId && userIds.Contains(f.UserId)))
            .ToListAsync();

        return users.Select(u => new UserWithRequestStatusResponse
        {
            User = UserResponse.FromUser(u),
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
    public async Task<List<UserMatchScoreWithStatusResponse>> GetSuggestedUsers(
        ClaimsPrincipal claimsPrincipal,
        int limit = 10)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var currentUserId))
        {
            return new List<UserMatchScoreWithStatusResponse>();
        }

        var currentUser = await GetCurrentUserWithSkillsAndInterests(currentUserId);
        if (currentUser == null)
        {
            return new List<UserMatchScoreWithStatusResponse>();
        }

        var blockedUserIds = await GetBlockedUserIds(currentUserId);
        var existingFriendIds = await GetExistingFriendIds(currentUserId);
        var currentUserProjectIds = await GetCurrentUserProjectIds(currentUserId);

        var allUsers = await GetPotentialUsers(currentUserId, blockedUserIds, existingFriendIds);

        var userProjectCounts = await GetUserProjectCounts(currentUserProjectIds, currentUserId);
        var usersWhoCommentedOnMyPosts = await GetUsersWhoCommentedOnMyPosts(currentUserId);
        var usersWhoCommentedOnSamePosts = await GetUsersWhoCommentedOnSamePosts(currentUserId);

        var userScores = CalculateMatchScores(
            allUsers, 
            currentUser, 
            userProjectCounts, 
            usersWhoCommentedOnMyPosts, 
            usersWhoCommentedOnSamePosts);

        var topMatches = userScores
            .Where(s => s.MatchScore > 0)
            .OrderByDescending(s => s.MatchScore)
            .Take(limit)
            .ToList();

        var pendingRequests = await GetPendingRequests(currentUserId, topMatches.Select(s => s.User.Id).ToList());

        return topMatches.Select(s => new UserMatchScoreWithStatusResponse
        {
            User = UserResponse.FromUser(s.User),
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

    private async Task<User?> GetCurrentUserWithSkillsAndInterests(int userId)
    {
        return await _context.Users
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    private async Task<List<int>> GetBlockedUserIds(int currentUserId)
    {
        return await _context.BlockedUsers
            .Where(bu => bu.UserId == currentUserId || bu.BlockedUserId == currentUserId)
            .Select(bu => bu.UserId == currentUserId ? bu.BlockedUserId : bu.UserId)
            .ToListAsync();
    }

    private async Task<List<int>> GetExistingFriendIds(int currentUserId)
    {
        return await _context.Friendships
            .Where(f => f.UserId == currentUserId || f.FriendId == currentUserId)
            .Select(f => f.UserId == currentUserId ? f.FriendId : f.UserId)
            .ToListAsync();
    }

    private async Task<List<int>> GetCurrentUserProjectIds(int currentUserId)
    {
        var ownedProjectIds = await _context.Projects
            .Where(p => p.OwnerId == currentUserId)
            .Select(p => p.Id)
            .ToListAsync();

        var collaboratedProjectIds = await _context.UserProjects
            .Where(up => up.UserId == currentUserId)
            .Select(up => up.ProjectId)
            .ToListAsync();

        return ownedProjectIds.Union(collaboratedProjectIds).ToList();
    }

    private async Task<List<User>> GetPotentialUsers(int currentUserId, List<int> blockedUserIds, List<int> existingFriendIds)
    {
        return await _context.Users
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => u.Id != currentUserId && 
                       !existingFriendIds.Contains(u.Id) && 
                       !blockedUserIds.Contains(u.Id))
            .ToListAsync();
    }

    private async Task<Dictionary<int, int>> GetUserProjectCounts(List<int> currentUserProjectIds, int currentUserId)
    {
        var collaborators = await _context.UserProjects
            .Where(up => currentUserProjectIds.Contains(up.ProjectId) && up.UserId != currentUserId)
            .Select(up => new { up.UserId, up.ProjectId })
            .ToListAsync();

        var owners = await _context.Projects
            .Where(p => currentUserProjectIds.Contains(p.Id) && p.OwnerId != currentUserId)
            .Select(p => new { UserId = p.OwnerId, ProjectId = p.Id })
            .ToListAsync();

        return collaborators
            .Concat(owners)
            .GroupBy(up => up.UserId)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private async Task<Dictionary<int, int>> GetUsersWhoCommentedOnMyPosts(int currentUserId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var currentUserPostIds = await _context.Posts
            .Where(p => p.UserId == currentUserId)
            .Select(p => p.Id)
            .ToListAsync();

        return await _context.PostComments
            .Where(pc => currentUserPostIds.Contains(pc.PostId) && 
                        pc.UserId != currentUserId &&
                        pc.CreatedAt >= thirtyDaysAgo)
            .GroupBy(pc => pc.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);
    }

    private async Task<Dictionary<int, int>> GetUsersWhoCommentedOnSamePosts(int currentUserId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var postsWhereICommented = await _context.PostComments
            .Where(pc => pc.UserId == currentUserId && pc.CreatedAt >= thirtyDaysAgo)
            .Select(pc => pc.PostId)
            .Distinct()
            .ToListAsync();

        return await _context.PostComments
            .Where(pc => postsWhereICommented.Contains(pc.PostId) && 
                        pc.UserId != currentUserId &&
                        pc.CreatedAt >= thirtyDaysAgo)
            .GroupBy(pc => pc.UserId)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);
    }

    private List<UserMatchScore> CalculateMatchScores(
        List<User> allUsers, 
        User currentUser, 
        Dictionary<int, int> userProjectCounts,
        Dictionary<int, int> usersWhoCommentedOnMyPosts,
        Dictionary<int, int> usersWhoCommentedOnSamePosts)
    {
        var currentUserSkills = currentUser.Skills.Select(s => s.SkillName).ToHashSet();
        var currentUserInterests = currentUser.Interests.Select(i => i.InterestName).ToHashSet();

        return allUsers.Select(user =>
        {
            var userSkills = user.Skills.Select(s => s.SkillName).ToHashSet();
            var userInterests = user.Interests.Select(i => i.InterestName).ToHashSet();

            var skillSimilarity = CalculateJaccardSimilarity(currentUserSkills, userSkills);
            var interestSimilarity = CalculateJaccardSimilarity(currentUserInterests, userInterests);

            var commonProjects = userProjectCounts.GetValueOrDefault(user.Id, 0);
            var projectScore = Math.Min(commonProjects * 0.2, 1.0);

            var commentsOnMyPosts = usersWhoCommentedOnMyPosts.GetValueOrDefault(user.Id, 0);
            var commentsOnSamePosts = usersWhoCommentedOnSamePosts.GetValueOrDefault(user.Id, 0);
            var totalInteractions = commentsOnMyPosts + commentsOnSamePosts;
            var interactionScore = Math.Min(totalInteractions * 0.1, 1.0);

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
                CommonSkills = currentUserSkills.Intersect(userSkills).Count(),
                CommonInterests = currentUserInterests.Intersect(userInterests).Count(),
                CommonProjects = commonProjects,
                RecentInteractions = totalInteractions
            };
        }).ToList();
    }

    private double CalculateJaccardSimilarity(HashSet<string> set1, HashSet<string> set2)
    {
        var common = set1.Intersect(set2).Count();
        var total = set1.Union(set2).Count();
        return total > 0 ? (double)common / total : 0;
    }

    private async Task<List<FriendRequest>> GetPendingRequests(int currentUserId, List<int> userIds)
    {
        return await _context.FriendRequests
            .Where(fr => 
                (fr.RequesterId == currentUserId && userIds.Contains(fr.RequesteeId)) ||
                (fr.RequesteeId == currentUserId && userIds.Contains(fr.RequesterId)))
            .ToListAsync();
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
