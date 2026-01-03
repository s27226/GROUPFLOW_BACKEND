using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

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