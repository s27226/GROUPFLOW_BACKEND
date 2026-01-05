using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Services;

public class ModerationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ModerationService> _logger;

    public ModerationService(AppDbContext context, ILogger<ModerationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private async Task<User> RequireModeratorAsync(int moderatorId, CancellationToken ct)
    {
        var moderator = await _context.Users.FindAsync(new object[] { moderatorId }, ct);
        if (moderator == null || !moderator.IsModerator)
        {
            _logger.LogWarning("User {ModeratorId} attempted a moderator action without permissions", moderatorId);
            throw new ForbiddenException("Only moderators can perform this action.");
        }

        return moderator;
    }

    private async Task<User> RequireUserAsync(int userId, CancellationToken ct)
    {
        var user = await _context.Users.FindAsync(new object[] { userId }, ct);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            throw new NotFoundException($"User {userId} not found.");
        }

        return user;
    }

    public async Task<User> BanUserAsync(int moderatorId, int userId, string reason, DateTime? expiresAt, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);
        var user = await RequireUserAsync(userId, ct);

        user.IsBanned = true;
        user.BanReason = reason;
        user.BanExpiresAt = expiresAt;
        user.BannedByUserId = moderatorId;

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} banned by {ModeratorId} for reason: {Reason}, expires at: {ExpiresAt}", userId, moderatorId, reason, expiresAt);

        return user;
    }

    public async Task<User> UnbanUserAsync(int moderatorId, int userId, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);
        var user = await RequireUserAsync(userId, ct);

        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;
        user.BannedByUserId = null;

        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} unbanned by {ModeratorId}", userId, moderatorId);

        return user;
    }

    public async Task<User> SuspendUserAsync(int moderatorId, int userId, DateTime until, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);
        var user = await RequireUserAsync(userId, ct);

        user.SuspendedUntil = until;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} suspended by {ModeratorId} until {Until}", userId, moderatorId, until);

        return user;
    }

    public async Task<User> UnsuspendUserAsync(int moderatorId, int userId, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);
        var user = await RequireUserAsync(userId, ct);

        user.SuspendedUntil = null;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} unsuspended by {ModeratorId}", userId, moderatorId);

        return user;
    }

    public async Task<bool> DeletePostAsync(int moderatorId, int postId, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);

        var post = await _context.Posts.FindAsync(new object[] { postId }, ct)
            ?? throw new NotFoundException("Post not found.");

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Post {PostId} deleted by moderator {ModeratorId}", postId, moderatorId);
        return true;
    }

    public async Task<bool> DeleteCommentAsync(int moderatorId, int commentId, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);

        var comment = await _context.PostComments.FindAsync(new object[] { commentId }, ct)
            ?? throw new NotFoundException("Comment not found.");

        _context.PostComments.Remove(comment);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Comment {CommentId} deleted by moderator {ModeratorId}", commentId, moderatorId);
        return true;
    }

    public async Task<bool> DeleteProjectAsync(int moderatorId, int projectId, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);

        var project = await _context.Projects.FindAsync(new object[] { projectId }, ct)
            ?? throw new NotFoundException("Project not found.");

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Project {ProjectId} deleted by moderator {ModeratorId}", projectId, moderatorId);
        return true;
    }

    public async Task<bool> DeleteChatAsync(int moderatorId, int chatId, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);

        var chat = await _context.Chats.FindAsync(new object[] { chatId }, ct)
            ?? throw new NotFoundException("Chat not found.");

        _context.Chats.Remove(chat);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Chat {ChatId} deleted by moderator {ModeratorId}", chatId, moderatorId);
        return true;
    }

    public async Task<User> ResetPasswordAsync(int moderatorId, int userId, string newPassword, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);
        var user = await RequireUserAsync(userId, ct);

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Password for user {UserId} reset by moderator {ModeratorId}", userId, moderatorId);

        return user;
    }

    public async Task<User> ManageUserRoleAsync(int moderatorId, int userId, bool isModerator, CancellationToken ct)
    {
        await RequireModeratorAsync(moderatorId, ct);
        var user = await RequireUserAsync(userId, ct);

        user.IsModerator = isModerator;
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} role changed to {IsModerator} by moderator {ModeratorId}", userId, isModerator, moderatorId);

        return user;
    }
}


public class NotFoundException : GraphQLException
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : GraphQLException
{
    public UnauthorizedException(string message) : base(message) { }
}

public class ForbiddenException : GraphQLException
{
    public ForbiddenException(string message) : base(message) { }
}