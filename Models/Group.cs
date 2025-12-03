namespace NAME_WIP_BACKEND.Models;

public class Group
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Desc { get; set; }
    public ICollection<UserGroup> Members { get; set; } = new List<UserGroup>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}