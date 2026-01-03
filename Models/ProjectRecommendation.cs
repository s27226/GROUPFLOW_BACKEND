using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;

public class ProjectRecommendation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public int RecValue { get; set; }

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
