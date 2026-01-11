using FluentValidation;
using GROUPFLOW.Models;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for user login input.
/// </summary>
public class UserLoginInputValidator : AbstractValidator<UserLoginInput>
{
    public UserLoginInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
