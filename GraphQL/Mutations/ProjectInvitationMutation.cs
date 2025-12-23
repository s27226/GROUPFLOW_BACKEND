using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using HotChocolate;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectInvitationMutation
{
    public ProjectInvitation CreateProjectInvitation(AppDbContext context, ProjectInvitationInput input)
    {
        Console.WriteLine($"[CreateProjectInvitation] ProjectId={input.ProjectId}, InvitingId={input.InvitingId}, InvitedId={input.InvitedId}");
        
        // Validate that the invited user is not already a member of the project
        var existingMembership = context.UserProjects
            .Any(up => up.ProjectId == input.ProjectId && up.UserId == input.InvitedId);
        
        Console.WriteLine($"[CreateProjectInvitation] existingMembership={existingMembership}");
        
        if (existingMembership)
        {
            throw new GraphQLException("User is already a member of this project.");
        }

        // Validate that the inviting user and invited user are friends
        var areFriends = context.Friendships
            .Any(f => f.IsAccepted && 
                     ((f.UserId == input.InvitingId && f.FriendId == input.InvitedId) ||
                      (f.UserId == input.InvitedId && f.FriendId == input.InvitingId)));
        
        Console.WriteLine($"[CreateProjectInvitation] areFriends={areFriends}");
        
        if (!areFriends)
        {
            throw new GraphQLException("You can only invite friends to your projects.");
        }

        // Check if invitation already exists
        var existingInvite = context.ProjectInvitations
            .Any(pi => pi.ProjectId == input.ProjectId && pi.InvitedId == input.InvitedId);
        
        Console.WriteLine($"[CreateProjectInvitation] existingInvite={existingInvite}");
        
        if (existingInvite)
        {
            throw new GraphQLException("An invitation to this project already exists for this user.");
        }

        var invite = new ProjectInvitation
        {
            ProjectId = input.ProjectId,
            InvitingId = input.InvitingId,
            InvitedId = input.InvitedId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.UtcNow.AddHours(3)
        };
        context.ProjectInvitations.Add(invite);
        context.SaveChanges();
        
        Console.WriteLine($"[CreateProjectInvitation] Invitation created successfully with ID={invite.Id}");
        
        // Reload with navigation properties to avoid null reference issues
        return context.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.Inviting)
            .Include(pi => pi.Invited)
            .First(pi => pi.Id == invite.Id);
    }

    public ProjectInvitation? UpdateProjectInvitation(AppDbContext context, UpdateProjectInvitationInput input)
    {
        var invite = context.ProjectInvitations.Find(input.Id);
        if (invite == null) return null;
        if (input.ProjectId.HasValue) invite.ProjectId = input.ProjectId.Value;
        if (input.InvitingId.HasValue) invite.InvitingId = input.InvitingId.Value;
        if (input.InvitedId.HasValue) invite.InvitedId = input.InvitedId.Value;
        context.SaveChanges();
        return invite;
    }

    public bool DeleteProjectInvitation(AppDbContext context, int id)
    {
        var invite = context.ProjectInvitations.Find(id);
        if (invite == null) return false;
        context.ProjectInvitations.Remove(invite);
        context.SaveChanges();
        return true;
    }

    public bool AcceptProjectInvitation(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        int invitationId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        // Find the invitation
        var invitation = context.ProjectInvitations
            .Include(pi => pi.Project)
                .ThenInclude(p => p.Chat)
            .FirstOrDefault(pi => pi.Id == invitationId);
        
        if (invitation == null)
        {
            throw new GraphQLException("Invitation not found");
        }

        // Check if the invitation is for the current user
        if (invitation.InvitedId != userId)
        {
            throw new GraphQLException("This invitation is not for you");
        }

        // Check if invitation has expired
        if (invitation.Expiring < DateTime.UtcNow)
        {
            context.ProjectInvitations.Remove(invitation);
            context.SaveChanges();
            throw new GraphQLException("This invitation has expired");
        }

        // Check if user is already a collaborator
        var existingCollaborator = context.UserProjects
            .FirstOrDefault(up => up.ProjectId == invitation.ProjectId && up.UserId == userId);
        
        if (existingCollaborator != null)
        {
            // User is already a collaborator, just remove the invitation
            context.ProjectInvitations.Remove(invitation);
            context.SaveChanges();
            return true;
        }

        // Add user as collaborator
        var userProject = new UserProject
        {
            UserId = userId,
            ProjectId = invitation.ProjectId,
            Role = "Collaborator",
            JoinedAt = DateTime.UtcNow
        };
        context.UserProjects.Add(userProject);

        // Add user to project chat if it exists
        if (invitation.Project.Chat != null)
        {
            var existingUserChat = context.UserChats
                .FirstOrDefault(uc => uc.ChatId == invitation.Project.Chat.Id && uc.UserId == userId);
            
            if (existingUserChat == null)
            {
                var userChat = new UserChat
                {
                    UserId = userId,
                    ChatId = invitation.Project.Chat.Id
                };
                context.UserChats.Add(userChat);
            }
        }

        // Remove the invitation
        context.ProjectInvitations.Remove(invitation);
        
        context.SaveChanges();
        return true;
    }

    public bool RejectProjectInvitation(
        AppDbContext context,
        IHttpContextAccessor httpContextAccessor,
        int invitationId)
    {
        var currentUser = httpContextAccessor.HttpContext!.User;
        var userIdClaim = currentUser.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if (userIdClaim == null)
        {
            throw new GraphQLException("User not authenticated");
        }
        
        int userId = int.Parse(userIdClaim);

        // Find the invitation
        var invitation = context.ProjectInvitations.Find(invitationId);
        
        if (invitation == null)
        {
            throw new GraphQLException("Invitation not found");
        }

        // Check if the invitation is for the current user
        if (invitation.InvitedId != userId)
        {
            throw new GraphQLException("This invitation is not for you");
        }

        // Remove the invitation
        context.ProjectInvitations.Remove(invitation);
        context.SaveChanges();
        return true;
    }
}
