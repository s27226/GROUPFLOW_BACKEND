using FluentValidation;
using GROUPFLOW.Features.Posts.Inputs;

namespace GROUPFLOW.Features.Posts.Validators;

/// <summary>
/// Validator for creating/updating posts.
/// </summary>
public class PostInputValidator : AbstractValidator<PostInput>
{
    public PostInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(100).WithMessage("Title cannot exceed 100 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required")
            .MinimumLength(10).WithMessage("Content must be at least 10 characters");

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("Description cannot exceed 300 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("ImageUrl must be a valid URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number")
            .When(x => x.ProjectId.HasValue);

        RuleFor(x => x.SharedPostId)
            .GreaterThan(0).WithMessage("SharedPostId must be a positive number")
            .When(x => x.SharedPostId.HasValue);
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var result)
               && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}

/// <summary>
/// Validator for reporting posts.
/// </summary>
public class ReportPostInputValidator : AbstractValidator<ReportPostInput>
{
    public ReportPostInputValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("PostId must be a positive number");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .Length(10, 500).WithMessage("Reason must be between 10 and 500 characters");
    }
}
