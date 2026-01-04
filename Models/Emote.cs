using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.Models;

public class Emote
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<EntryReaction> Reactions { get; set; } = new List<EntryReaction>();
}