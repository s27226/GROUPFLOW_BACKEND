using GROUPFLOW.Common.Database;
using GROUPFLOW.Features.Projects.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Features.Projects.GraphQL.Queries;

public class ProjectInvitationQuery
{
    [GraphQLName("allprojectinvitations")]
    public async Task<List<ProjectInvitation>> GetProjectInvitations(
        AppDbContext context,
        ClaimsPrincipal claimsPrincipal)
    {
        var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return new List<ProjectInvitation>();
        }

        // Return project invitations where the current user is the invitee (received invitations)
        return await context.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.Inviting)
                .ThenInclude(u => u.ProfilePicBlob)
            .Include(pi => pi.Invited)
                .ThenInclude(u => u.ProfilePicBlob)
            .Where(pi => pi.InvitedId == userId)
            .ToListAsync();
    }
    
    [GraphQLName("projectinvitationbyid")]
    public async Task<ProjectInvitation?> GetProjectInvitationById(AppDbContext context, int id)
    {
        return await context.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.Inviting)
                .ThenInclude(u => u.ProfilePicBlob)
            .Include(pi => pi.Invited)
                .ThenInclude(u => u.ProfilePicBlob)
            .FirstOrDefaultAsync(g => g.Id == id);
    }
}
