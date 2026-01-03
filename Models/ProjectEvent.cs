using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

[Index(nameof(ProjectId))]
public class ProjectEvent
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int CreatedById { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string? Time { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Project Project { get; set; } = null!;
    public User CreatedBy { get; set; } = null!;
}
