namespace NAME_WIP_BACKEND.DTOs;

/// <summary>
/// DTO for friendship-related operations to isolate from DB model
/// </summary>
public class FriendshipDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int FriendId { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FriendDto
{
    public int Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Surname { get; set; }
    public string? ProfilePic { get; set; }
    public string? Bio { get; set; }
}

public class FriendshipStatusDto
{
    public string Status { get; set; } = string.Empty; // "none", "friends", "pending_sent", "pending_received"
    public bool CanSendRequest { get; set; }
    public bool CanAcceptRequest { get; set; }
}
