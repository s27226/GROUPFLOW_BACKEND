using System.Security.Claims;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.GraphQL.Inputs;

public class UsersQuery
{
    [GraphQLName("allusers")]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> GetUsers([Service] AppDbContext context) => context.Users;
    
    [GraphQLName("getuserbyid")]
    [UseProjection]
    public User? GetUserById(AppDbContext context, int id) => context.Users.FirstOrDefault(g => g.Id == id);
    
    [Authorize]
    [GraphQLName("me")]
    [UseProjection]
    public User? GetCurrentUser(AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        
        return context.Users.FirstOrDefault(u => u.Id == userId);
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

            // Weighted score: interests slightly more important than skills for friend matching
            var matchScore = (skillSimilarity * 0.4 + interestSimilarity * 0.6) * 100;

            return new UserMatchScore
            {
                User = user,
                MatchScore = matchScore,
                CommonSkills = commonSkills,
                CommonInterests = commonInterests
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
}

public class UserMatchScoreWithStatus
{
    public User User { get; set; } = null!;
    public double MatchScore { get; set; }
    public int CommonSkills { get; set; }
    public int CommonInterests { get; set; }
    public bool HasPendingRequest { get; set; }
}

public class UserWithRequestStatus
{
    public User User { get; set; } = null!;
    public bool HasPendingRequest { get; set; }
    public bool IsFriend { get; set; }
}
