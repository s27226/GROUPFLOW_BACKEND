using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;
[Index(nameof(UserId))]
public class UserSkill
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string SkillName { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
