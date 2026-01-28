using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Features.Users.Entities;
using System.Security.Claims;

namespace GROUPFLOW.Features.Moderation.GraphQL.Queries;

public class ModerationQuery
{
    public async Task<List<User>> GetAllUsers([Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw AuthorizationException.NotModerator();
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
            throw AuthorizationException.NotModerator();
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
            throw AuthorizationException.NotModerator();
        }

        return await context.Users
            .Include(u => u.BannedBy)
            .Where(u => u.SuspendedUntil != null && u.SuspendedUntil > DateTime.UtcNow)
            .OrderBy(u => u.Id)
            .ToListAsync();
    }
}
