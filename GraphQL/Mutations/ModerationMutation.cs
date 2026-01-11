using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using BCrypt.Net;
using System.Security.Claims;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ModerationMutation
{
    public async Task<User> BanUser(BanUserInput input, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        // Validate input using DataAnnotations
        input.ValidateInput();
        
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can ban users.");
        }

        var user = await context.Users.FindAsync(input.UserId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.IsBanned = true;
        user.BanReason = input.Reason;
        user.BanExpiresAt = input.ExpiresAt;
        user.BannedByUserId = moderatorId;

        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UnbanUser(int userId, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can unban users.");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.IsBanned = false;
        user.BanReason = null;
        user.BanExpiresAt = null;
        user.BannedByUserId = null;

        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> SuspendUser(SuspendUserInput input, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        // Validate input using DataAnnotations
        input.ValidateInput();
        
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can suspend users.");
        }

        var user = await context.Users.FindAsync(input.UserId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.SuspendedUntil = input.SuspendedUntil;

        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UnsuspendUser(int userId, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can unsuspend users.");
        }

        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.SuspendedUntil = null;

        await context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> DeletePost(int postId, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can delete posts.");
        }

        var post = await context.Posts.FindAsync(postId);
        if (post == null)
        {
            throw new Exception("Post not found.");
        }

        context.Posts.Remove(post);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteComment(int commentId, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can delete comments.");
        }

        var comment = await context.PostComments.FindAsync(commentId);
        if (comment == null)
        {
            throw new Exception("Comment not found.");
        }

        context.PostComments.Remove(comment);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteProject(int projectId, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can delete projects.");
        }

        var project = await context.Projects.FindAsync(projectId);
        if (project == null)
        {
            throw new Exception("Project not found.");
        }

        context.Projects.Remove(project);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteChat(int chatId, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can delete chats.");
        }

        var chat = await context.Chats.FindAsync(chatId);
        if (chat == null)
        {
            throw new Exception("Chat not found.");
        }

        context.Chats.Remove(chat);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<User> ResetPassword(ResetPasswordInput input, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        // Validate input using DataAnnotations
        input.ValidateInput();
        
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can reset passwords.");
        }

        var user = await context.Users.FindAsync(input.UserId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(input.NewPassword);

        await context.SaveChangesAsync();
        return user;
    }

    public async Task<User> ManageUserRole(ManageUserRoleInput input, [Service] AppDbContext context, ClaimsPrincipal claimsPrincipal)
    {
        // Validate input using DataAnnotations
        input.ValidateInput();
        
        var moderatorId = int.Parse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
        var moderator = await context.Users.FindAsync(moderatorId);
        
        if (moderator == null || !moderator.IsModerator)
        {
            throw new UnauthorizedAccessException("Only moderators can manage user roles.");
        }

        var user = await context.Users.FindAsync(input.UserId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.IsModerator = input.IsModerator;

        await context.SaveChangesAsync();
        return user;
    }
}
