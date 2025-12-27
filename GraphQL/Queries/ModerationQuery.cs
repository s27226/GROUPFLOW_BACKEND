using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ModerationQuery
{
    public async Task<List<User>> GetAllUsers([Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can view all users.");
        }

        return await context.Users
            .Include(u => u.BannedBy)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<List<User>> GetBannedUsers([Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can view banned users.");
        }

        return await context.Users
            .Include(u => u.BannedBy)
            .Where(u => u.IsBanned)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }

    public async Task<List<User>> GetSuspendedUsers([Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can view suspended users.");
        }

        return await context.Users
            .Include(u => u.BannedBy)
            .Where(u => u.SuspendedUntil != null && u.SuspendedUntil > DateTime.UtcNow)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }
}
