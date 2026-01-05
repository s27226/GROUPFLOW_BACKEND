using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using NAME_WIP_BACKEND.Services;
using BCrypt.Net;
using System.Security.Claims;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ModerationMutation
{
    
    private readonly ModerationService _service;

    public ModerationMutation(ModerationService service)
    {
        _service = service;
    }
    
    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedException("Not authenticated"));

    public Task<User> BanUser(
        BanUserInput input,
        ClaimsPrincipal user,
        CancellationToken ct)
        => _service.BanUserAsync(GetUserId(user), input.UserId, input.Reason, input.ExpiresAt, ct);

    public Task<User> UnbanUser(int userId, ClaimsPrincipal user, CancellationToken ct)
        => _service.UnbanUserAsync(GetUserId(user), userId, ct);

    public Task<User> SuspendUser(SuspendUserInput input, ClaimsPrincipal user, CancellationToken ct)
        => _service.SuspendUserAsync(GetUserId(user), input.UserId, input.SuspendedUntil, ct);

    public Task<User> UnsuspendUser(int userId, ClaimsPrincipal user, CancellationToken ct)
        => _service.UnsuspendUserAsync(GetUserId(user), userId, ct);

    public Task<bool> DeletePost(int postId, ClaimsPrincipal user, CancellationToken ct)
        => _service.DeletePostAsync(GetUserId(user), postId, ct);

    public Task<bool> DeleteComment(int commentId, ClaimsPrincipal user, CancellationToken ct)
        => _service.DeleteCommentAsync(GetUserId(user), commentId, ct);

    public Task<bool> DeleteProject(int projectId, ClaimsPrincipal user, CancellationToken ct)
        => _service.DeleteProjectAsync(GetUserId(user), projectId, ct);

    public Task<bool> DeleteChat(int chatId, ClaimsPrincipal user, CancellationToken ct)
        => _service.DeleteChatAsync(GetUserId(user), chatId, ct);

    public Task<User> ResetPassword(ResetPasswordInput input, ClaimsPrincipal user, CancellationToken ct)
        => _service.ResetPasswordAsync(GetUserId(user), input.UserId, input.NewPassword, ct);

    public Task<User> ManageUserRole(ManageUserRoleInput input, ClaimsPrincipal user, CancellationToken ct)
        => _service.ManageUserRoleAsync(GetUserId(user), input.UserId, input.IsModerator, ct);
}