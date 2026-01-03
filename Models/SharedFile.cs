using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

public class SharedFile
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public string Link { get; set; } = null!;

    public Chat Chat { get; set; } = null!;
}