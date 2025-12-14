namespace NAME_WIP_BACKEND.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }
    
    // Navigation property
    public User User { get; set; } = null!;
}
