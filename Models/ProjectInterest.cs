using Microsoft.EntityFrameworkCore;

namespace GROUPFLOW.Models;
[Index(nameof(ProjectId))]
public class ProjectInterest
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string InterestName { get; set; } = null!;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
