using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;

namespace NAME_WIP_BACKEND.Services;

public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId, CancellationToken ct)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null)
        {
            _logger.LogWarning(
                "User {UserId} attempted to mark non-existing notification {NotificationId} as read",
                userId,
                notificationId);
            throw new NotFoundException("Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Notification {NotificationId} marked as read by user {UserId}",
                notificationId,
                userId);
        }
        else
        {
            _logger.LogInformation(
                "Notification {NotificationId} already marked as read by user {UserId}",
                notificationId,
                userId);
        }

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId, CancellationToken ct)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        if (!notifications.Any())
        {
            _logger.LogInformation(
                "User {UserId} attempted to mark all notifications as read, but none were unread",
                userId);
            return true;
        }

        foreach (var notification in notifications)
            notification.IsRead = true;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "All notifications marked as read by user {UserId} (count: {Count})",
            userId,
            notifications.Count);

        return true;
    }
}
