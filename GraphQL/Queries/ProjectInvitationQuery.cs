using NAME_WIP_BACKEND.Data;
using NAME_WIP_BACKEND.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.GraphQL.Queries;

public class ProjectInvitationQuery
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ProjectInvitationQuery(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    [GraphQLName("allprojectinvitations")]
    [UsePaging]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProjectInvitation> GetProjectInvitations()
    {
        var claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        var userIdClaim = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Enumerable.Empty<ProjectInvitation>().AsQueryable();
        }

        // Return project invitations where the current user is the invitee (received invitations)
        return _context.ProjectInvitations
            .Include(pi => pi.Project)
            .Include(pi => pi.Inviting)
            .Include(pi => pi.Invited)
            .Where(pi => pi.InvitedId == userId);
    }
    
    [GraphQLName("projectinvitationbyid")]
    [UseProjection]
    public ProjectInvitation? GetProjectInvitationById(int id) => _context.ProjectInvitations.FirstOrDefault(g => g.Id == id);
}