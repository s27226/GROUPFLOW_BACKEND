using FluentValidation;
using NAME_WIP_BACKEND.GraphQL.Inputs;

namespace NAME_WIP_BACKEND.GraphQL.Validators;

/// <summary>
/// Validator for chat entry input.
/// </summary>
public class EntryInputValidator : AbstractValidator<EntryInput>
{
    public EntryInputValidator()
    {
        RuleFor(x => x.UserChatId)
            .GreaterThan(0).WithMessage("UserChatId must be a positive number");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required")
            .MaximumLength(500).WithMessage("Message cannot exceed 500 characters");
    }
}
