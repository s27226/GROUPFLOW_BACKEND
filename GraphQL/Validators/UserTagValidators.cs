using FluentValidation;
using GROUPFLOW.GraphQL.Inputs;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for user skill input.
/// </summary>
public class UserSkillInputValidator : AbstractValidator<UserSkillInput>
{
    public UserSkillInputValidator()
    {
        RuleFor(x => x.SkillName)
            .NotEmpty().WithMessage("SkillName is required")
            .MaximumLength(50).WithMessage("SkillName cannot exceed 50 characters");
    }
}

/// <summary>
/// Validator for user interest input.
/// </summary>
public class UserInterestInputValidator : AbstractValidator<UserInterestInput>
{
    public UserInterestInputValidator()
    {
        RuleFor(x => x.InterestName)
            .NotEmpty().WithMessage("InterestName is required")
            .MaximumLength(50).WithMessage("InterestName cannot exceed 50 characters");
    }
}

/// <summary>
/// Validator for searching users input.
/// </summary>
public class SearchUsersInputValidator : AbstractValidator<SearchUsersInput>
{
    public SearchUsersInputValidator()
    {
        RuleFor(x => x.SearchTerm)
            .MaximumLength(100).WithMessage("SearchTerm cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.Skills)
            .Must(skills => skills == null || skills.Count <= 10)
            .WithMessage("Cannot search for more than 10 skills");

        RuleFor(x => x.Interests)
            .Must(interests => interests == null || interests.Count <= 10)
            .WithMessage("Cannot search for more than 10 interests");
    }
}
