using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Exceptions;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using BCrypt.Net;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for moderation operations.
/// All methods require moderator privileges.
/// </summary>
public class ModerationMutation
{
    private readonly ILogger<ModerationMutation> _logger;

    public ModerationMutation(ILogger<ModerationMutation> logger)
    {
        _logger = logger;
    }

    private static async Task<User> GetModeratorAsync(AppDbContext context, ClaimsPrincipal claimsPrincipal, CancellationToken ct)
    {
        var moderatorId = claimsPrincipal.GetAuthenticatedUserId();
        var moderator = await context.Users.FindAsync(new object[] { moderatorId }, ct)
            ?? throw EntityNotFoundException.User(moderatorId);

        if (!moderator.IsModerator)
            throw new AuthorizationException("Only moderators can perform this action");

        return moderator;
    }

    public async Task<User> BanUser(
        BanUserInput input,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var user = await context.Users.FindAsync(new object[] { input.UserId }, ct)
            ?? throw EntityNotFoundException.User(input.UserId);

        user.IsBanned = true;
        user.BanReason = input.Reason;
        user.BanExpiresAt = input.ExpiresAt;
        user.BannedByUserId = moderator.Id;

        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} banned user {UserId}: {Reason}", moderator.Id, input.UserId, input.Reason);
        return user;
    }

    public async Task<User> UnbanUser(
        int userId,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var user = await context.Users.FindAsync(new object[] { userId }, ct)
            ?? throw EntityNotFoundException.User(userId);

        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;
        user.BannedByUserId = null;

        await context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Moderator {ModeratorId} unbanned user {UserId}", moderator.Id, userId);
        return user;
    }

    public async Task<User> SuspendUser(
        SuspendUserInput input,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var user = await context.Users.FindAsync(new object[] { input.UserId }, ct)
            ?? throw EntityNotFoundException.User(input.UserId);

        user.SuspendedUntil = input.SuspendedUntil;

        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} suspended user {UserId} until {Until}", moderator.Id, input.UserId, input.SuspendedUntil);
        return user;
    }

    public async Task<User> UnsuspendUser(
        int userId,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var user = await context.Users.FindAsync(new object[] { userId }, ct)
            ?? throw EntityNotFoundException.User(userId);

        user.SuspendedUntil = null;

        await context.SaveChangesAsync(ct);
        
        _logger.LogInformation("Moderator {ModeratorId} unsuspended user {UserId}", moderator.Id, userId);
        return user;
    }

    public async Task<bool> DeletePost(
        int postId,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var post = await context.Posts.FindAsync(new object[] { postId }, ct)
            ?? throw EntityNotFoundException.Post(postId);

        context.Posts.Remove(post);
        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} deleted post {PostId}", moderator.Id, postId);
        return true;
    }

    public async Task<bool> DeleteComment(
        int commentId,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var comment = await context.PostComments.FindAsync(new object[] { commentId }, ct)
            ?? throw EntityNotFoundException.Comment(commentId);

        context.PostComments.Remove(comment);
        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} deleted comment {CommentId}", moderator.Id, commentId);
        return true;
    }

    public async Task<bool> DeleteProject(
        int projectId,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var project = await context.Projects.FindAsync(new object[] { projectId }, ct)
            ?? throw EntityNotFoundException.Project(projectId);

        context.Projects.Remove(project);
        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} deleted project {ProjectId}", moderator.Id, projectId);
        return true;
    }

    public async Task<bool> DeleteChat(
        int chatId,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var chat = await context.Chats.FindAsync(new object[] { chatId }, ct)
            ?? throw EntityNotFoundException.Chat(chatId);

        context.Chats.Remove(chat);
        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} deleted chat {ChatId}", moderator.Id, chatId);
        return true;
    }

    public async Task<User> ResetPassword(
        ResetPasswordInput input,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var user = await context.Users.FindAsync(new object[] { input.UserId }, ct)
            ?? throw EntityNotFoundException.User(input.UserId);

        user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);

        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} reset password for user {UserId}", moderator.Id, input.UserId);
        return user;
    }

    public async Task<User> ManageUserRole(
        ManageUserRoleInput input,
        [Service] AppDbContext context,
        ClaimsPrincipal claimsPrincipal,
        CancellationToken ct = default)
    {
        input.ValidateInput();
        var moderator = await GetModeratorAsync(context, claimsPrincipal, ct);

        var user = await context.Users.FindAsync(new object[] { input.UserId }, ct)
            ?? throw EntityNotFoundException.User(input.UserId);

        user.IsModerator = input.IsModerator;

        await context.SaveChangesAsync(ct);
        
        _logger.LogWarning("Moderator {ModeratorId} changed role for user {UserId}: IsModerator={IsModerator}", 
            moderator.Id, input.UserId, input.IsModerator);
        return user;
    }
}
