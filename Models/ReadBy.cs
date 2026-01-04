using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;
[Index(nameof(UserId), nameof(EntryId), IsUnique = true)]
public class ReadBy
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public int UserId { get; set; }

    public Entry Entry { get; set; } = null!;
    public User User { get; set; } = null!;
}