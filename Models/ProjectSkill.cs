using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

public class ProjectSkill
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string SkillName { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
