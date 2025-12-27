namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public class PostInput
{
    public string? Title { get; set; }
    public string Content { get; set; } = null!;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int? ProjectId { get; set; }
    public int? SharedPostId { get; set; }
    public bool IsPublic { get; set; } = true;
}
