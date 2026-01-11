using FluentValidation;
using GROUPFLOW.GraphQL.Inputs;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for creating a project.
/// </summary>
public class ProjectInputValidator : AbstractValidator<ProjectInput>
{
    public ProjectInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required")
            .MaximumLength(500).WithMessage("Project description cannot exceed 500 characters");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("ImageUrl must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.Skills)
            .Must(skills => skills == null || skills.Length <= 10)
            .WithMessage("Cannot have more than 10 skills");

        RuleFor(x => x.Interests)
            .Must(interests => interests == null || interests.Length <= 10)
            .WithMessage("Cannot have more than 10 interests");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for creating a project with members.
/// </summary>
public class CreateProjectWithMembersInputValidator : AbstractValidator<CreateProjectWithMembersInput>
{
    public CreateProjectWithMembersInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Project description is required")
            .MaximumLength(500).WithMessage("Project description cannot exceed 500 characters");

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("ImageUrl must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.MemberUserIds)
            .NotEmpty().WithMessage("At least one member is required")
            .Must(ids => ids.All(id => id > 0)).WithMessage("All member IDs must be positive numbers");

        RuleFor(x => x.Skills)
            .Must(skills => skills == null || skills.Length <= 10)
            .WithMessage("Cannot have more than 10 skills");

        RuleFor(x => x.Interests)
            .Must(interests => interests == null || interests.Length <= 10)
            .WithMessage("Cannot have more than 10 interests");
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for updating a project.
/// </summary>
public class UpdateProjectInputValidator : AbstractValidator<UpdateProjectInput>
{
    public UpdateProjectInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Project ID must be a positive number");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Project description cannot exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("ImageUrl must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}
