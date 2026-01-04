using Microsoft.EntityFrameworkCore;

namespace GroupFlow_BACKEND.Models;

public class UserRole
{
    public int Id { get; set; }
    public string RoleName { get; set; } = null!;
    public ICollection<User> Users { get; set; } = new List<User>();
}