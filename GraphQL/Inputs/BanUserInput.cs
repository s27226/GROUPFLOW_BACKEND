using System.ComponentModel.DataAnnotations;
namespace GROUPFLOW.GraphQL.Inputs;

public record BanUserInput(
    [property: Range(1, int.MaxValue)]
    int UserId,
    [property: Required]
    [property: StringLength(300, MinimumLength = 5)]
    string Reason,
    
    DateTime? ExpiresAt
);
