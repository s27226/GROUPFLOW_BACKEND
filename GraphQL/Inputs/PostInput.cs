using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public class PostInput
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
    public string? Title { get; set; }
    
    [Required(ErrorMessage = "Content is required")]
    [MinLength(10, ErrorMessage = "Content must be at least 10 characters long")]
    public string Content { get; set; } = null!;
    
    [StringLength(300, ErrorMessage = "Description cannot exceed 300 characters")]
    public string? Description { get; set; }
    
    
    [Url(ErrorMessage = "ImageUrl must be a valid URL")]
    public string? ImageUrl { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "ProjectId must be a positive number")]
    public int? ProjectId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "SharedPostId must be a positive number")]
    public int? SharedPostId { get; set; }
    public bool IsPublic { get; set; } = true;
}
