using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models
{
    [Index(nameof(ProjectId), nameof(UserId), IsUnique = true)]
    public class ProjectLike
    {
        public int Id { get; set; }
        
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
