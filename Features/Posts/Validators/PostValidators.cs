using FluentValidation;
using GROUPFLOW.Features.Posts.GraphQL.Inputs;

namespace GROUPFLOW.Features.Posts.Validators;

/// <summary>
/// Validator for creating/updating posts.
/// </summary>
public class PostInputValidator : AbstractValidator<PostInput>
{
    public PostInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("errors.TITLE_REQUIRED")
            .MaximumLength(100).WithMessage("errors.TITLE_TOO_LONG");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("errors.CONTENT_REQUIRED")
            .MinimumLength(10).WithMessage("errors.CONTENT_TOO_SHORT");

        RuleFor(x => x.Description)
            .MaximumLength(300).WithMessage("errors.DESCRIPTION_TOO_LONG")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ImageUrl)
            .Must(BeAValidUrl).WithMessage("errors.INVALID_URL")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("errors.INVALID_PROJECT_ID")
            .When(x => x.ProjectId.HasValue);

        RuleFor(x => x.SharedPostId)
            .GreaterThan(0).WithMessage("errors.INVALID_SHARED_POST_ID")
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
            .GreaterThan(0).WithMessage("errors.INVALID_POST_ID");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("errors.REASON_REQUIRED")
            .Length(10, 500).WithMessage("errors.REASON_INVALID_LENGTH");
    }
}
