using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models;
[Index(nameof(UserId), nameof(EmoteId),nameof(EntryId), IsUnique = true)]
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