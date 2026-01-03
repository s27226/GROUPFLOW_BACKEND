using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

public class UserInterest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string InterestName { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
