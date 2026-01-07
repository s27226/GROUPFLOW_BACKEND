using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using NAME_WIP_BACKEND.GraphQL.Responses;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ModerationQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ModerationQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<UserResponse>> GetAllUsers()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var moderatorId = int.Parse(claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await _context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can view all users.");
        }

        return (await _context.Users
            .Include(u => u.BannedBy)
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .OrderBy(u => u.Id)
            .ToListAsync())
            .Select(u => UserResponse.FromUser(u))
            .ToList();
    }

    public async Task<List<UserResponse>> GetBannedUsers()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var moderatorId = int.Parse(claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await _context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can view banned users.");
        }

        return (await _context.Users
            .Include(u => u.BannedBy)
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => u.IsBanned)
            .OrderBy(u => u.Id)
            .ToListAsync())
            .Select(u => UserResponse.FromUser(u))
            .ToList();
    }

    public async Task<List<UserResponse>> GetSuspendedUsers()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var moderatorId = int.Parse(claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await _context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can view suspended users.");
        }

        return (await _context.Users
            .Include(u => u.BannedBy)
            .Include(u => u.ProfilePicBlob)
            .Include(u => u.BannerPicBlob)
            .Include(u => u.Skills)
            .Include(u => u.Interests)
            .Where(u => u.SuspendedUntil > DateTime.UtcNow)
            .OrderBy(u => u.Id)
            .ToListAsync())
            .Select(u => UserResponse.FromUser(u))
            .ToList();
    }
}
