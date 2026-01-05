using System.Security.Claims;
using NAME_WIP_BACKEND.Services;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class NotificationMutation
{
    private readonly NotificationService _service;

    public NotificationMutation(NotificationService service)
    {
        _service = service;
    }

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedException("Not authenticated"));

    [GraphQLName("markNotificationAsRead")]
    public Task<bool> MarkNotificationAsRead(
        int notificationId,
        ClaimsPrincipal user,
        CancellationToken ct)
        => _service.MarkAsReadAsync(GetUserId(user), notificationId, ct);

    [GraphQLName("markAllNotificationsAsRead")]
    public Task<bool> MarkAllNotificationsAsRead(
        ClaimsPrincipal user,
        CancellationToken ct)
        => _service.MarkAllAsReadAsync(GetUserId(user), ct);
}