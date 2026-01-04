using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.Models;

public class ReadBy
{
    public int Id { get; set; }
    public int EntryId { get; set; }
    public int UserId { get; set; }

    public Entry Entry { get; set; } = null!;
    public User User { get; set; } = null!;
}