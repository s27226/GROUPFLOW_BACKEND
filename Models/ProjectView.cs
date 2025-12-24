namespace NAME_WIP_BACKEND.Models
{
    public class ProjectView
    {
        public int Id { get; set; }
        
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        
        // Track the date (not time) for daily uniqueness
        public DateTime ViewDate { get; set; }
        
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}
