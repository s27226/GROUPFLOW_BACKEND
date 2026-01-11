using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Nickname), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class User
{public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string? ProfilePic { get; set; }
    public string? BannerPic { get; set; }
    
    // Blob storage references
    public int? ProfilePicBlobId { get; set; }
    public BlobFile? ProfilePicBlob { get; set; }
    public int? BannerPicBlobId { get; set; }
    public BlobFile? BannerPicBlob { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public DateTime Joined { get; set; } = DateTime.UtcNow;
    public bool IsModerator { get; set; } = false;

    // Moderation fields
    public bool IsBanned { get; set; } = false;
    public string? BanReason { get; set; }
    public DateTime? BanExpiresAt { get; set; }
    public DateTime? SuspendedUntil { get; set; }
    public int? BannedByUserId { get; set; }
    public User? BannedBy { get; set; }

    public int? UserRoleId { get; set; }
    public UserRole? UserRole { get; set; }

    public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
    public ICollection<EntryReaction> EntryReactions { get; set; } = new List<EntryReaction>();
    public ICollection<ReadBy> ReadBys { get; set; } = new List<ReadBy>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<UserProject> ProjectCollaborations { get; set; } = new List<UserProject>();
    public ICollection<Friendship> InitiatedFriendships { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendships { get; set; } = new List<Friendship>();
    public ICollection<ProjectEvent> CreatedEvents { get; set; } = new List<ProjectEvent>();
    public ICollection<SavedPost> SavedPosts { get; set; } = new List<SavedPost>();
    public ICollection<UserSkill> Skills { get; set; } = new List<UserSkill>();
    public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
    
}
