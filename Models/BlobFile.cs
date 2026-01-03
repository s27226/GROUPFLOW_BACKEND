using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models
{
    public enum BlobType
    {
        UserProfilePicture,
        UserBanner,
        ProjectLogo,
        ProjectBanner,
        ProjectFile,
        PostImage
    }

    [Index(nameof(UploadedByUserId))]
    [Index(nameof(ProjectId))]
    [Index(nameof(PostId))]
    public class BlobFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = null!;
        public string BlobPath { get; set; } = null!; // Path in S3 bucket
        public string ContentType { get; set; } = null!;
        public long FileSize { get; set; } // Size in bytes
        public BlobType Type { get; set; }
        
        // Owner information
        public int UploadedByUserId { get; set; }
        public User UploadedBy { get; set; } = null!;
        
        // Optional project reference for project files
        public int? ProjectId { get; set; }
        public Project? Project { get; set; }
        
        // Optional post reference for post images
        public int? PostId { get; set; }
        public Post? Post { get; set; }
        
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
