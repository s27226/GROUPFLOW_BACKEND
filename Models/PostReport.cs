using Microsoft.EntityFrameworkCore;

namespace NAME_WIP_BACKEND.Models;
[Index(nameof(PostId))]
public class PostReport
{
    public int Id { get; set; }
    
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    
    public int ReportedBy { get; set; }
    public User ReportedByUser { get; set; } = null!;
    
    public string Reason { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsResolved { get; set; } = false;
}
