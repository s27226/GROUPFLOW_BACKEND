using FluentValidation;
using GROUPFLOW.GraphQL.Inputs;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for banning a user.
/// </summary>
public class BanUserInputValidator : AbstractValidator<BanUserInput>
{
    public BanUserInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required")
            .MinimumLength(5).WithMessage("Reason must be at least 5 characters")
            .MaximumLength(300).WithMessage("Reason cannot exceed 300 characters");

        RuleFor(x => x.ExpiresAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("ExpiresAt must be in the future")
            .When(x => x.ExpiresAt.HasValue);
    }
}

/// <summary>
/// Validator for suspending a user.
/// </summary>
public class SuspendUserInputValidator : AbstractValidator<SuspendUserInput>
{
    public SuspendUserInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");

        RuleFor(x => x.SuspendedUntil)
            .NotEmpty().WithMessage("SuspendedUntil is required")
            .GreaterThan(DateTime.UtcNow).WithMessage("SuspendedUntil must be in the future");
    }
}

/// <summary>
/// Validator for managing user roles.
/// </summary>
public class ManageUserRoleInputValidator : AbstractValidator<ManageUserRoleInput>
{
    public ManageUserRoleInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");
    }
}

/// <summary>
/// Validator for resetting user password.
/// </summary>
public class ResetPasswordInputValidator : AbstractValidator<ResetPasswordInput>
{
    public ResetPasswordInputValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be a positive number");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("NewPassword is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");
    }
}
