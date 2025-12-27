using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class NotificationMutation
{
    [GraphQLName("markNotificationAsRead")]
    public async Task<bool> MarkNotificationAsRead(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int notificationId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null)
        {
            throw new GraphQLException("Notification not found");
        }

        notification.IsRead = true;
        await context.SaveChangesAsync();

        return true;
    }

    [GraphQLName("markAllNotificationsAsRead")]
    public async Task<bool> MarkAllNotificationsAsRead(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        int userId = int.Parse(currentUser.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var notifications = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await context.SaveChangesAsync();

        return true;
    }
}
