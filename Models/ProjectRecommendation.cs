using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models;
[Index(nameof(UserId), nameof(ProjectId), IsUnique = true)]
public class ProjectRecommendation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProjectId { get; set; }
    public int RecValue { get; set; }

    public User User { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
