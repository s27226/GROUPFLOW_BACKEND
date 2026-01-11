using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Features.Users.Entities;

namespace GROUPFLOW.Features.Chat.Entities;

[Index(nameof(ProjectId))]
public class Chat
{
    public int Id { get; set; }
    
    public int? ProjectId { get; set; }
    public Projects.Entities.Project? Project { get; set; }
    
    public ICollection<UserChat> UserChats { get; set; } = new List<UserChat>();
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
    public ICollection<SharedFile> SharedFiles { get; set; } = new List<SharedFile>();
}

[Index(nameof(ChatId))]
[Index(nameof(UserId))]
public class UserChat
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int UserId { get; set; }

    public Chat Chat { get; set; } = null!;
    public User User { get; set; } = null!;
    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}

[Index(nameof(UserChatId))]
public class Entry
{
    public int Id { get; set; }
    public int UserChatId { get; set; }
    public string Message { get; set; } = null!;
    public DateTime Sent { get; set; }
    public bool Public { get; set; }

    public UserChat UserChat { get; set; } = null!;
    public ICollection<EntryReaction> Reactions { get; set; } = new List<EntryReaction>();
    public ICollection<ReadBy> ReadBys { get; set; } = new List<ReadBy>();
}

[Index(nameof(UserId), nameof(EmoteId), nameof(EntryId), IsUnique = true)]
public class EntryReaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EntryId { get; set; }
    public int EmoteId { get; set; }

    public User User { get; set; } = null!;
    public Entry Entry { get; set; } = null!;
    public Emote Emote { get; set; } = null!;
}

[Index(nameof(UserId), nameof(EntryId), IsUnique = true)]
public class ReadBy
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public int UserId { get; set; }

    public Entry Entry { get; set; } = null!;
    public User User { get; set; } = null!;
}

[Index(nameof(ChatId))]
public class SharedFile
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Link { get; set; } = null!;

    public Chat Chat { get; set; } = null!;
}

public class Emote
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<EntryReaction> Reactions { get; set; } = new List<EntryReaction>();
}
