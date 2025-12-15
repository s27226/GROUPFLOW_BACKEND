namespace NAME_WIP_BACKEND.GraphQL.Inputs;

public record UserSkillInput(string SkillName);

public record UserInterestInput(string InterestName);

public record SearchUsersInput
{
    public string? SearchTerm { get; init; }
    public List<string>? Skills { get; init; }
    public List<string>? Interests { get; init; }
}
