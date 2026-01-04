using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.Models
{
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