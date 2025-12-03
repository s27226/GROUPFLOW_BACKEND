namespace NAME_WIP_BACKEND.Models
{
    public class Post
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }

        public string Content { get; set; } = null!;

        public bool Public { get; set; } = true;

        public DateTime Created { get; set; }
    }
}
