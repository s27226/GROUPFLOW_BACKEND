using FluentValidation;
using NAME_WIP_BACKEND.Models;

namespace NAME_WIP_BACKEND.GraphQL.Validators;

/// <summary>
/// Validator for user registration input.
/// Validation rules are defined here and automatically executed via FluentValidation middleware.
/// </summary>
public class UserRegisterInputValidator : AbstractValidator<UserRegisterInput>
{
    public UserRegisterInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters");

        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Surname is required")
            .MaximumLength(50).WithMessage("Surname cannot exceed 50 characters");

        RuleFor(x => x.Nickname)
            .NotEmpty().WithMessage("Nickname is required")
            .MaximumLength(30).WithMessage("Nickname cannot exceed 30 characters")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Nickname can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches(@"[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches(@"[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches(@"[0-9]").WithMessage("Password must contain at least one number");
    }
}
