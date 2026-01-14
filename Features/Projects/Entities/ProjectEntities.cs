using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Features.Blobs.Entities;
using GROUPFLOW.Features.Chat.Entities;
using GROUPFLOW.Features.Posts.Entities;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Projects.Entities;

[Index(nameof(OwnerId))]
[Index(nameof(Name))]
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? Image { get; set; }
    public string? Banner { get; set; }
    
    // Blob storage references
    public int? ImageBlobId { get; set; }
    public BlobFile? ImageBlob { get; set; }
    public int? BannerBlobId { get; set; }
    public BlobFile? BannerBlob { get; set; }
    
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsPublic { get; set; } = true;
    
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    
    public ICollection<UserProject> Collaborators { get; set; } = new List<UserProject>();
    public ICollection<ProjectEvent> Events { get; set; } = new List<ProjectEvent>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<ProjectView> Views { get; set; } = new List<ProjectView>();
    public ICollection<ProjectSkill> Skills { get; set; } = new List<ProjectSkill>();
    public ICollection<ProjectInterest> Interests { get; set; } = new List<ProjectInterest>();
    public Chat.Entities.Chat? Chat { get; set; }
}

[Index(nameof(UserId), nameof(ProjectId), IsUnique = true)]
public class UserProject
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public string Role { get; set; } = "Collaborator";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

[Index(nameof(ProjectId))]
public class ProjectEvent
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int CreatedById { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? Time { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}

[Index(nameof(ProjectId), nameof(InvitingId), nameof(InvitedId), IsUnique = true)]
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

[Index(nameof(UserId), nameof(ProjectId), IsUnique = true)]
public class ProjectRecommendation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public int RecValue { get; set; }

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}

[Index(nameof(UserId), nameof(ProjectId), nameof(ViewDate), IsUnique = true)]
public class ProjectView
{
    public int Id { get; set; }
    
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime ViewDate { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
}

[Index(nameof(ProjectId))]
public class ProjectSkill
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string SkillName { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}

[Index(nameof(ProjectId))]
public class ProjectInterest
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string InterestName { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
