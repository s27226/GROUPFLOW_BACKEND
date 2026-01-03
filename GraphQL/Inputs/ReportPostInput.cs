using System.ComponentModel.DataAnnotations;
namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public class ReportPostInput
{
    [Range(1, int.MaxValue)]
    public int PostId { get; set; }

    [Required]
    [StringLength(300, MinimumLength = 5)]
    public string Reason { get; set; } = null!;
}
