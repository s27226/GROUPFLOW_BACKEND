using System.ComponentModel.DataAnnotations;

namespace GROUPFLOW.Features.Posts.GraphQL.Inputs;

public class PostInput
{
    [Required(ErrorMessage = "errors.TITLE_REQUIRED")]
    [StringLength(100, ErrorMessage = "errors.TITLE_TOO_LONG")]
    public string? Title { get; set; }
    
    [Required(ErrorMessage = "errors.CONTENT_REQUIRED")]
    [MinLength(10, ErrorMessage = "errors.CONTENT_TOO_SHORT")]
    public string Content { get; set; } = null!;
    
    [StringLength(300, ErrorMessage = "errors.DESCRIPTION_TOO_LONG")]
    public string? Description { get; set; }
    
    [Url(ErrorMessage = "errors.INVALID_URL")]
    public string? ImageUrl { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "errors.INVALID_PROJECT_ID")]
    public int? ProjectId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "errors.INVALID_SHARED_POST_ID")]
    public int? SharedPostId { get; set; }
    
    public bool IsPublic { get; set; } = true;
}

public class ReportPostInput
{
    [Required(ErrorMessage = "errors.POST_ID_REQUIRED")]
    [Range(1, int.MaxValue, ErrorMessage = "errors.INVALID_POST_ID")]
    public int PostId { get; set; }
    
    [Required(ErrorMessage = "errors.REASON_REQUIRED")]
    [StringLength(500, MinimumLength = 10, ErrorMessage = "errors.REASON_INVALID_LENGTH")]
    public string Reason { get; set; } = null!;
}
