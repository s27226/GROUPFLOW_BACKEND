using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Notifications.Entities;

namespace GROUPFLOW.Features.Notifications.GraphQL;

/// <summary>
/// GraphQL mutations for notification operations.
/// </summary>
public class NotificationMutation
{
    private readonly ILogger<NotificationMutation> _logger;

    public NotificationMutation(ILogger<NotificationMutation> logger)
    {
        _logger = logger;
    }

    [GraphQLName("markNotificationAsRead")]
    public async Task<bool> MarkNotificationAsRead(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int notificationId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct)
            ?? throw EntityNotFoundException.Notification(notificationId);

        notification.IsRead = true;
        await context.SaveChangesAsync(ct);

        return true;
    }

    [GraphQLName("markAllNotificationsAsRead")]
    public async Task<bool> MarkAllNotificationsAsRead(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var notifications = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await context.SaveChangesAsync(ct);

        _logger.LogDebug("User {UserId} marked {Count} notifications as read", userId, notifications.Count);
        return true;
    }
}
