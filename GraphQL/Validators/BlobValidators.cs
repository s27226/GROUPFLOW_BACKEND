using FluentValidation;
using GROUPFLOW.GraphQL.Inputs;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for uploading blobs.
/// </summary>
public class UploadBlobInputValidator : AbstractValidator<UploadBlobInput>
{
    private static readonly string[] ValidBlobTypes = 
    { 
        "UserProfilePicture", 
        "UserBanner", 
        "ProjectLogo", 
        "ProjectBanner", 
        "ProjectFile", 
        "PostImage" 
    };

    public UploadBlobInputValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required")
            .MaximumLength(255).WithMessage("FileName cannot exceed 255 characters");

        RuleFor(x => x.Base64Data)
            .NotEmpty().WithMessage("Base64Data is required");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required")
            .MaximumLength(100).WithMessage("ContentType cannot exceed 100 characters");

        RuleFor(x => x.BlobType)
            .NotEmpty().WithMessage("BlobType is required")
            .MaximumLength(50).WithMessage("BlobType cannot exceed 50 characters")
            .Must(x => ValidBlobTypes.Contains(x))
            .WithMessage($"BlobType must be one of: {string.Join(", ", ValidBlobTypes)}");

        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number")
            .When(x => x.ProjectId.HasValue);

        RuleFor(x => x.PostId)
            .GreaterThan(0).WithMessage("PostId must be a positive number")
            .When(x => x.PostId.HasValue);
    }
}

/// <summary>
/// Validator for deleting blobs.
/// </summary>
public class DeleteBlobInputValidator : AbstractValidator<DeleteBlobInput>
{
    public DeleteBlobInputValidator()
    {
        RuleFor(x => x.BlobId)
            .GreaterThan(0).WithMessage("BlobId must be a positive number");
    }
}

/// <summary>
/// Validator for updating user profile image.
/// </summary>
public class UpdateUserProfileImageInputValidator : AbstractValidator<UpdateUserProfileImageInput>
{
    public UpdateUserProfileImageInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");

        RuleFor(x => x.ProfilePicBlobId)
            .GreaterThan(0).WithMessage("ProfilePicBlobId must be a positive number")
            .When(x => x.ProfilePicBlobId.HasValue);
    }
}

/// <summary>
/// Validator for updating user banner image.
/// </summary>
public class UpdateUserBannerImageInputValidator : AbstractValidator<UpdateUserBannerImageInput>
{
    public UpdateUserBannerImageInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");

        RuleFor(x => x.BannerPicBlobId)
            .GreaterThan(0).WithMessage("BannerPicBlobId must be a positive number")
            .When(x => x.BannerPicBlobId.HasValue);
    }
}

/// <summary>
/// Validator for updating project image.
/// </summary>
public class UpdateProjectImageInputValidator : AbstractValidator<UpdateProjectImageInput>
{
    public UpdateProjectImageInputValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number");

        RuleFor(x => x.ImageBlobId)
            .GreaterThan(0).WithMessage("ImageBlobId must be a positive number")
            .When(x => x.ImageBlobId.HasValue);
    }
}

/// <summary>
/// Validator for updating project banner.
/// </summary>
public class UpdateProjectBannerInputValidator : AbstractValidator<UpdateProjectBannerInput>
{
    public UpdateProjectBannerInputValidator()
    {
        RuleFor(x => x.ProjectId)
            .GreaterThan(0).WithMessage("ProjectId must be a positive number");

        RuleFor(x => x.BannerBlobId)
            .GreaterThan(0).WithMessage("BannerBlobId must be a positive number")
            .When(x => x.BannerBlobId.HasValue);
    }
}
