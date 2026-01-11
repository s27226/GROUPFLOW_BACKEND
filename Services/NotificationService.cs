using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Data;
using GROUPFLOW.Exceptions;
using GROUPFLOW.Models;

namespace GROUPFLOW.Services;

/// <summary>
/// Service for notification-related operations.
/// Handles creation, retrieval, and management of user notifications.
/// </summary>
public class NotificationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(AppDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Notification> CreateNotificationAsync(
        int userId,
        string type,
        string message,
        int? actorUserId = null,
        int? postId = null,
        CancellationToken ct = default)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Message = message,
            ActorUserId = actorUserId,
            PostId = postId,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("Created notification {NotificationId} for user {UserId}: {Type}", notification.Id, userId, type);

        return notification;
    }

    public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(
        int userId,
        bool unreadOnly = false,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt);

        if (unreadOnly)
        {
            query = (IOrderedQueryable<Notification>)query.Where(n => !n.IsRead);
        }

        return await query
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken ct = default)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, int userId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null)
            throw EntityNotFoundException.Notification(notificationId);

        notification.IsRead = true;
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);

        return true;
    }

    public async Task<int> MarkAllAsReadAsync(int userId, CancellationToken ct = default)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Marked {Count} notifications as read for user {UserId}", unreadNotifications.Count, userId);

        return unreadNotifications.Count;
    }

    public async Task<bool> DeleteNotificationAsync(int notificationId, int userId, CancellationToken ct = default)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId, ct);

        if (notification == null)
            throw EntityNotFoundException.Notification(notificationId);

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug("Deleted notification {NotificationId} for user {UserId}", notificationId, userId);

        return true;
    }

    // Helper methods for creating specific notification types
    public Task<Notification> NotifyPostLikeAsync(int postOwnerId, int likerUserId, int postId, string likerName, CancellationToken ct = default)
    {
        return CreateNotificationAsync(
            postOwnerId,
            AppConstants.NotificationTypeLike,
            $"{likerName} liked your post",
            likerUserId,
            postId,
            ct: ct);
    }

    public Task<Notification> NotifyCommentAsync(int postOwnerId, int commenterUserId, int postId, string commenterName, CancellationToken ct = default)
    {
        return CreateNotificationAsync(
            postOwnerId,
            AppConstants.NotificationTypeComment,
            $"{commenterName} commented on your post",
            commenterUserId,
            postId,
            ct: ct);
    }

    public Task<Notification> NotifyFriendRequestAsync(int targetUserId, int requesterUserId, string requesterName, CancellationToken ct = default)
    {
        return CreateNotificationAsync(
            targetUserId,
            AppConstants.NotificationTypeFriendRequest,
            $"{requesterName} sent you a friend request",
            requesterUserId,
            ct: ct);
    }

    public Task<Notification> NotifyProjectInviteAsync(int targetUserId, int inviterUserId, int projectId, string inviterName, string projectName, CancellationToken ct = default)
    {
        return CreateNotificationAsync(
            targetUserId,
            AppConstants.NotificationTypeProjectInvite,
            $"{inviterName} invited you to join {projectName}",
            inviterUserId,
            ct: ct);
    }
}
