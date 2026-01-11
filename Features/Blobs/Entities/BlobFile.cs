using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Features.Posts.Entities;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Blobs.Entities;

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
    public string BlobPath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }
    public BlobType Type { get; set; }
    
    public int UploadedByUserId { get; set; }
    public User UploadedBy { get; set; } = null!;
    
    public int? ProjectId { get; set; }
    public Projects.Entities.Project? Project { get; set; }
    
    public int? PostId { get; set; }
    public Post? Post { get; set; }
    
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
