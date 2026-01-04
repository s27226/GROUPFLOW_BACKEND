using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models
{
    [Index(nameof(UserId), nameof(ProjectId), IsUnique = true)]
    public class UserProject
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public string Role { get; set; } = "Collaborator"; // Owner, Admin, Collaborator, Viewer
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}