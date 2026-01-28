using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using GROUPFLOW.Common.Database;
using GROUPFLOW.Common.Exceptions;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Projects.Entities;
using GROUPFLOW.Features.Projects.GraphQL.Inputs;
using GROUPFLOW.Features.Chat.Entities;

namespace GROUPFLOW.Features.Projects.GraphQL.Mutations;

/// <summary>
/// GraphQL mutations for project invitation operations.
/// </summary>
public class ProjectInvitationMutation
{
    private readonly ILogger<ProjectInvitationMutation> _logger;

    public ProjectInvitationMutation(ILogger<ProjectInvitationMutation> logger)
    {
        _logger = logger;
    }

    public async Task<ProjectInvitation> CreateProjectInvitation(
        [Service] AppDbContext context,
        ProjectInvitationInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        // Validate that the invited user is not already a member
        var existingMembership = await context.UserProjects
            .AnyAsync(up => up.ProjectId == input.ProjectId && up.UserId == input.InvitedId, ct);

        if (existingMembership)
            throw BusinessRuleException.UserAlreadyProjectMember();

        // Validate that inviting and invited users are friends
        var areFriends = await context.Friendships
            .AnyAsync(f => f.IsAccepted &&
                ((f.UserId == input.InvitingId && f.FriendId == input.InvitedId) ||
                 (f.UserId == input.InvitedId && f.FriendId == input.InvitingId)), ct);

        if (!areFriends)
            throw BusinessRuleException.CanOnlyInviteFriends();

        // Check if invitation already exists
        var existingInvite = await context.ProjectInvitations
            .AnyAsync(pi => pi.ProjectId == input.ProjectId && pi.InvitedId == input.InvitedId, ct);

        if (existingInvite)
            throw DuplicateEntityException.ProjectInvitation();

        var invite = new ProjectInvitation
        {
            ProjectId = input.ProjectId,
            InvitingId = input.InvitingId,
            InvitedId = input.InvitedId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(3)
        };

        context.ProjectInvitations.Add(invite);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {InvitingId} invited user {InvitedId} to project {ProjectId}",
            input.InvitingId, input.InvitedId, input.ProjectId);

        return await context.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.Inviting)
            .Include(pi => pi.Invited)
            .FirstAsync(pi => pi.Id == invite.Id, ct);
    }

    public async Task<ProjectInvitation?> UpdateProjectInvitation(
        [Service] AppDbContext context,
        UpdateProjectInvitationInput input,
        CancellationToken ct = default)
    {
        input.ValidateInput();

        var invite = await context.ProjectInvitations.FindAsync(new object[] { input.Id }, ct);
        if (invite == null) return null;

        if (input.ProjectId.HasValue) invite.ProjectId = input.ProjectId.Value;
        if (input.InvitingId.HasValue) invite.InvitingId = input.InvitingId.Value;
        if (input.InvitedId.HasValue) invite.InvitedId = input.InvitedId.Value;

        await context.SaveChangesAsync(ct);
        return invite;
    }

    public async Task<bool> DeleteProjectInvitation(
        [Service] AppDbContext context,
        int id,
        CancellationToken ct = default)
    {
        var invite = await context.ProjectInvitations.FindAsync(new object[] { id }, ct);
        if (invite == null) return false;

        context.ProjectInvitations.Remove(invite);
        await context.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> AcceptProjectInvitation(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int invitationId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var invitation = await context.ProjectInvitations
            .Include(pi => pi.Project).ThenInclude(p => p.Chat)
            .FirstOrDefaultAsync(pi => pi.Id == invitationId, ct)
            ?? throw EntityNotFoundException.ProjectInvitation(invitationId);

        if (invitation.InvitedId != userId)
            throw AuthorizationException.NotInvitationRecipient();

        if (invitation.Expiring < DateTime.UtcNow)
        {
            context.ProjectInvitations.Remove(invitation);
            await context.SaveChangesAsync(ct);
            throw BusinessRuleException.InvitationExpired();
        }

        var strategy = context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(ct);

            var existingCollaborator = await context.UserProjects
                .FirstOrDefaultAsync(up => up.ProjectId == invitation.ProjectId && up.UserId == userId, ct);

            if (existingCollaborator != null)
            {
                context.ProjectInvitations.Remove(invitation);
                await context.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return;
            }

            context.UserProjects.Add(new UserProject
            {
                UserId = userId,
                ProjectId = invitation.ProjectId,
                Role = "Collaborator",
                JoinedAt = DateTime.UtcNow
            });

            if (invitation.Project.Chat != null)
            {
                var existingUserChat = await context.UserChats
                    .FirstOrDefaultAsync(uc => uc.ChatId == invitation.Project.Chat.Id && uc.UserId == userId, ct);

                if (existingUserChat == null)
                {
                    context.UserChats.Add(new UserChat
                    {
                        UserId = userId,
                        ChatId = invitation.Project.Chat.Id
                    });
                }
            }

            context.ProjectInvitations.Remove(invitation);
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        });

        _logger.LogInformation("User {UserId} accepted invitation {InvitationId} to project {ProjectId}",
            userId, invitationId, invitation.ProjectId);
        return true;
    }

    public async Task<bool> RejectProjectInvitation(
        [Service] AppDbContext context,
        [Service] IHttpContextAccessor httpContextAccessor,
        int invitationId,
        CancellationToken ct = default)
    {
        var userId = httpContextAccessor.GetAuthenticatedUserId();

        var invitation = await context.ProjectInvitations.FindAsync(new object[] { invitationId }, ct)
            ?? throw EntityNotFoundException.ProjectInvitation(invitationId);

        if (invitation.InvitedId != userId)
            throw AuthorizationException.NotInvitationRecipient();

        context.ProjectInvitations.Remove(invitation);
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("User {UserId} rejected invitation {InvitationId}", userId, invitationId);
        return true;
    }
}
