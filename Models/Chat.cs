using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

public class Chat
{
    public int Id { get; set; }
    
    /// <summary>
    /// ProjectId for project chats. Null for direct messages between friends.
    /// </summary>
    public int? ProjectId { get; set; }

    public Project? Project { get; set; }
    public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    public ICollection<SharedFile> SharedFiles { get; set; } = new List<SharedFile>();
}