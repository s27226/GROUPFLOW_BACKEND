namespace NAME_WIP_BACKEND.Models;

public class User
{public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public string Nickname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public DateTime? DateOfBirth { get; set; }
    public DateTime Joined { get; set; } = DateTime.UtcNow;

    public int? UserRoleId { get; set; }
    public UserRole? UserRole { get; set; }

    public ICollection<UserGroup> UserGroups { get; set; } = new List<UserGroup>();
    public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
    public ICollection<EntryReaction> EntryReactions { get; set; } = new List<EntryReaction>();
    public ICollection<ReadBy> ReadBys { get; set; } = new List<ReadBy>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
    public ICollection<UserProject> ProjectCollaborations { get; set; } = new List<UserProject>();
    public ICollection<Friendship> InitiatedFriendships { get; set; } = new List<Friendship>();
    public ICollection<Friendship> ReceivedFriendships { get; set; } = new List<Friendship>();
    
}
