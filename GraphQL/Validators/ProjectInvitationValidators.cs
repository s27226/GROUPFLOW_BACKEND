using FluentValidation;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Validators;

/// <summary>
/// Validator for project invitation input.
/// </summary>
public class ProjectInvitationInputValidator : AbstractValidator<ProjectInvitationInput>
{
    public ProjectInvitationInputValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number");

        RuleFor(x => x.InvitingId)
            .GreaterThan(0).WithMessage("InvitingId must be a positive number");

        RuleFor(x => x.InvitedId)
            .GreaterThan(0).WithMessage("InvitedId must be a positive number");

        RuleFor(x => x)
            .Must(x => x.InvitingId != x.InvitedId)
            .WithMessage("Cannot invite yourself to a project");
    }
}

/// <summary>
/// Validator for updating project invitation input.
/// </summary>
public class UpdateProjectInvitationInputValidator : AbstractValidator<UpdateProjectInvitationInput>
{
    public UpdateProjectInvitationInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a positive number");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number")
            .When(x => x.ProjectId.HasValue);

        RuleFor(x => x.InvitingId)
            .GreaterThan(0).WithMessage("InvitingId must be a positive number")
            .When(x => x.InvitingId.HasValue);

        RuleFor(x => x.InvitedId)
            .GreaterThan(0).WithMessage("InvitedId must be a positive number")
            .When(x => x.InvitedId.HasValue);
    }
}
