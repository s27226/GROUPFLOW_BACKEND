using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Notifications.Entities;

namespace GROUPFLOW.Features.Notifications.GraphQL.Queries;

public class NotificationQuery
{
    [GraphQLName("myNotifications")]
    public async Task<List<Notification>> GetMyNotifications(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int limit = 5)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var notifications = await context.Notifications
            .Include(n => n.ActorUser)
            .Include(n => n.Post)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();

        return notifications;
    }

    [GraphQLName("unreadNotificationsCount")]
    public async Task<int> GetUnreadNotificationsCount(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var count = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();

        return count;
    }
}
