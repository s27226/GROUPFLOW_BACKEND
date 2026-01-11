using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models;
[Index(nameof(ProjectId), nameof(InvitingId),nameof(InvitedId), IsUnique = true)]
public class ProjectInvitation
{
    public int Id { get; set; }
    public DateTime Sent { get; set; }
    public DateTime Expiring { get; set; }

    public int ProjectId { get; set; }
    public int InvitingId { get; set; }
    public int InvitedId { get; set; }

    public Project Project { get; set; } = null!;
    public User Inviting { get; set; } = null!;
    public User Invited { get; set; } = null!;
}
