using System.ComponentModel.DataAnnotations;
namespace GroupFlow_BACKEND.GraphQL.Inputs;

public record UserSkillInput(
    [property: Required]
    [property: StringLength(50)]
    string SkillName
);

public record UserInterestInput(
    [property: Required]
    [property: StringLength(50)]
    string InterestName
);

public record SearchUsersInput
{
    [StringLength(100)]
    public string? SearchTerm { get; init; }
    public List<string>? Skills { get; init; }
    public List<string>? Interests { get; init; }
}
