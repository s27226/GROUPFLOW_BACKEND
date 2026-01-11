namespace GROUPFLOW.Features.Moderation.Entities;

// Moderation-related entities are part of User entity (IsBanned, BanReason, etc.)
// This file is for any additional moderation-specific entities if needed

/// <summary>
/// DTO for moderation actions tracking (optional extension)
/// </summary>
public class ModerationAction
{
    public int Id { get; set; }
    public int ModeratorId { get; set; }
    public int TargetUserId { get; set; }
    public string ActionType { get; set; } = null!; // BAN, SUSPEND, WARN, UNBAN
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
