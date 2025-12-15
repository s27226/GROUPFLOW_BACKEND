using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.GraphQL.Inputs;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Mutations;

public class ProjectInvitationMutation
{
    public ProjectInvitation CreateProjectInvitation(AppDbContext context, ProjectInvitationInput input)
    {
        var invite = new ProjectInvitation
        {
            ProjectId = input.ProjectId,
            InvitingId = input.InvitingId,
            InvitedId = input.InvitedId,
            Sent = DateTime.UtcNow,
            Expiring = DateTime.Now.AddHours(3)
        };
        context.ProjectInvitations.Add(invite);
        context.SaveChanges();
        return invite;
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
}
