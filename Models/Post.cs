using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.Models
{
    [Index(nameof(UserId))]
    [Index(nameof(ProjectId))]
    [Index(nameof(Created))]
    public class Post
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? ImageUrl { get; set; }
        
        // Blob storage reference
        public int? ImageBlobId { get; set; }
        public BlobFile? ImageBlob { get; set; }

        public string Content { get; set; } = null!;

        public bool Public { get; set; } = true;

        public DateTime Created { get; set; }

        // Self-referencing relationship for shared posts
        public int? SharedPostId { get; set; }
        public Post? SharedPost { get; set; }

        public ICollection<SavedPost> SavedBy { get; set; } = new List<SavedPost>();
        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
        
        [GraphQLIgnore]
        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();
    }
}
