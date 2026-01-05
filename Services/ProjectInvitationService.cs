using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.Services;

public class ProjectInvitationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProjectInvitationService> _logger;

    public ProjectInvitationService(AppDbContext context, ILogger<ProjectInvitationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private static int GetUserId(ClaimsPrincipal user)
        => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<ProjectInvitation> CreateInvitation(
        ClaimsPrincipal user,
        ProjectInvitationInput input)
    {
        int invitingUserId = GetUserId(user);

        if (input.InvitingId != invitingUserId)
            throw new GraphQLException("You cannot invite on behalf of another user");

        bool alreadyMember = await _context.UserProjects
            .AnyAsync(up => up.ProjectId == input.ProjectId && up.UserId == input.InvitedId);

        if (alreadyMember)
            throw new GraphQLException("User is already a member of this project");

        bool areFriends = await _context.Friendships.AnyAsync(f =>
            f.IsAccepted &&
            ((f.UserId == invitingUserId && f.FriendId == input.InvitedId) ||
             (f.UserId == input.InvitedId && f.FriendId == invitingUserId)));

        if (!areFriends)
            throw new GraphQLException("You can only invite friends to your projects");

        bool inviteExists = await _context.ProjectInvitations.AnyAsync(pi =>
            pi.ProjectId == input.ProjectId && pi.InvitedId == input.InvitedId);

        if (inviteExists)
            throw new GraphQLException("An invitation already exists");

        var invitation = new ProjectInvitation
        {
            ProjectId = input.ProjectId,
            InvitingId = invitingUserId,
            InvitedId = input.InvitedId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(3)
        };

        _context.ProjectInvitations.Add(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {InvitingId} created invitation {InvitationId} for user {InvitedId} in project {ProjectId}",
            invitingUserId, invitation.Id, input.InvitedId, input.ProjectId);

        return await _context.ProjectInvitations
            .Include(i => i.Project)
            .Include(i => i.Inviting)
            .Include(i => i.Invited)
            .FirstAsync(i => i.Id == invitation.Id);
    }

    public async Task<bool> AcceptInvitation(ClaimsPrincipal user, int invitationId)
    {
        int userId = GetUserId(user);

        var invitation = await _context.ProjectInvitations
            .Include(i => i.Project)
                .ThenInclude(p => p.Chat)
            .FirstOrDefaultAsync(i => i.Id == invitationId);

        if (invitation == null)
            throw new GraphQLException("Invitation not found");

        if (invitation.InvitedId != userId)
            throw new GraphQLException("This invitation is not for you");

        if (invitation.Expiring < DateTime.UtcNow)
        {
            _context.ProjectInvitations.Remove(invitation);
            await _context.SaveChangesAsync();
            throw new GraphQLException("This invitation has expired");
        }

        bool alreadyCollaborator = await _context.UserProjects.AnyAsync(up =>
            up.ProjectId == invitation.ProjectId && up.UserId == userId);

        if (!alreadyCollaborator)
        {
            _context.UserProjects.Add(new UserProject
            {
                UserId = userId,
                ProjectId = invitation.ProjectId,
                Role = "Collaborator",
                JoinedAt = DateTime.UtcNow
            });

            if (invitation.Project.Chat != null)
            {
                bool alreadyInChat = await _context.UserChats.AnyAsync(uc =>
                    uc.ChatId == invitation.Project.Chat.Id && uc.UserId == userId);

                if (!alreadyInChat)
                {
                    _context.UserChats.Add(new UserChat
                    {
                        UserId = userId,
                        ChatId = invitation.Project.Chat.Id
                    });
                }
            }
        }

        _context.ProjectInvitations.Remove(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} accepted invitation {InvitationId} to project {ProjectId}",
            userId, invitationId, invitation.ProjectId);

        return true;
    }

    public async Task<bool> RejectInvitation(ClaimsPrincipal user, int invitationId)
    {
        int userId = GetUserId(user);

        var invitation = await _context.ProjectInvitations.FindAsync(invitationId);
        if (invitation == null)
            throw new GraphQLException("Invitation not found");

        if (invitation.InvitedId != userId)
            throw new GraphQLException("This invitation is not for you");

        _context.ProjectInvitations.Remove(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "User {UserId} rejected invitation {InvitationId} to project {ProjectId}",
            userId, invitationId, invitation.ProjectId);

        return true;
    }

    public async Task<bool> DeleteInvitation(int invitationId)
    {
        var invitation = await _context.ProjectInvitations.FindAsync(invitationId);
        if (invitation == null)
            return false;

        _context.ProjectInvitations.Remove(invitation);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Invitation {InvitationId} deleted from project {ProjectId}",
            invitationId, invitation.ProjectId);

        return true;
    }

    public async Task<ProjectInvitation> UpdateInvitation(UpdateProjectInvitationInput input)
    {
        var invitation = await _context.ProjectInvitations.FindAsync(input.Id);
        if (invitation == null)
            throw new GraphQLException("Invitation not found");

        if (input.ProjectId.HasValue)
            invitation.ProjectId = input.ProjectId.Value;

        if (input.InvitingId.HasValue)
            invitation.InvitingId = input.InvitingId.Value;

        if (input.InvitedId.HasValue)
            invitation.InvitedId = input.InvitedId.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Invitation {InvitationId} updated: Project={ProjectId}, Inviting={InvitingId}, Invited={InvitedId}",
            invitation.Id, invitation.ProjectId, invitation.InvitingId, invitation.InvitedId);

        return invitation;
    }
}
