using FluentValidation;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Validators;

/// <summary>
/// Validator for project recommendation input.
/// </summary>
public class ProjectRecommendationInputValidator : AbstractValidator<ProjectRecommendationInput>
{
    public ProjectRecommendationInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number");

        RuleFor(x => x.RecValue)
            .InclusiveBetween(1, 5).WithMessage("RecValue must be between 1 and 5");
    }
}

/// <summary>
/// Validator for updating project recommendation input.
/// </summary>
public class UpdateProjectRecommendationInputValidator : AbstractValidator<UpdateProjectRecommendationInput>
{
    public UpdateProjectRecommendationInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a positive number");

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number")
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number")
            .When(x => x.ProjectId.HasValue);

        RuleFor(x => x.RecValue)
            .InclusiveBetween(1, 5).WithMessage("RecValue must be between 1 and 5")
            .When(x => x.RecValue.HasValue);
    }
}
