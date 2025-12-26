namespace NAME_WIP_BACKEND.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public bool IsPublic { get; set; } = true;
        
        // Foreign keys
        public int OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        
        // Navigation properties
        public ICollection<UserProject> Collaborators { get; set; } = new List<UserProject>();
        public ICollection<ProjectEvent> Events { get; set; } = new List<ProjectEvent>();
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<ProjectLike> Likes { get; set; } = new List<ProjectLike>();
        public ICollection<ProjectView> Views { get; set; } = new List<ProjectView>();
        public ICollection<ProjectSkill> Skills { get; set; } = new List<ProjectSkill>();
        public ICollection<ProjectInterest> Interests { get; set; } = new List<ProjectInterest>();
        public Chat? Chat { get; set; }
    }
}