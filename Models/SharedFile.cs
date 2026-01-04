using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;
[Index(nameof(ChatId))]
public class SharedFile
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Link { get; set; } = null!;

    public Chat Chat { get; set; } = null!;
}