using FluentValidation;
using GROUPFLOW.GraphQL.Inputs;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for creating project events.
/// </summary>
public class ProjectEventInputValidator : AbstractValidator<ProjectEventInput>
{
    public ProjectEventInputValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number");

        RuleFor(x => x.CreatedById)
            .GreaterThan(0).WithMessage("CreatedById must be a positive number");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("Description cannot exceed 300 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.EventDate)
            .NotEmpty().WithMessage("EventDate is required");

        RuleFor(x => x.Time)
            .MaximumLength(10).WithMessage("Time cannot exceed 10 characters")
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Time must be in HH:mm format")
            .When(x => !string.IsNullOrEmpty(x.Time));
    }
}

/// <summary>
/// Validator for updating project events.
/// </summary>
public class UpdateProjectEventInputValidator : AbstractValidator<UpdateProjectEventInput>
{
    public UpdateProjectEventInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a positive number");

        RuleFor(x => x.Title)
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("Description cannot exceed 300 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.Time)
            .MaximumLength(10).WithMessage("Time cannot exceed 10 characters")
            .Matches(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$").WithMessage("Time must be in HH:mm format")
            .When(x => !string.IsNullOrEmpty(x.Time));
    }
}
