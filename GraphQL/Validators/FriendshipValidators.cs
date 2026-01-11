using FluentValidation;
using GROUPFLOW.GraphQL.Inputs;

namespace GROUPFLOW.GraphQL.Validators;

/// <summary>
/// Validator for friend request input.
/// </summary>
public class FriendRequestInputValidator : AbstractValidator<FriendRequestInput>
{
    public FriendRequestInputValidator()
    {
        RuleFor(x => x.RequesterId)
            .GreaterThan(0).WithMessage("RequesterId must be a positive number");

        RuleFor(x => x.RequesteeId)
            .GreaterThan(0).WithMessage("RequesteeId must be a positive number");

        RuleFor(x => x)
            .Must(x => x.RequesterId != x.RequesteeId)
            .WithMessage("Cannot send friend request to yourself");
    }
}

/// <summary>
/// Validator for updating friend request input.
/// </summary>
public class UpdateFriendRequestInputValidator : AbstractValidator<UpdateFriendRequestInput>
{
    public UpdateFriendRequestInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id must be a positive number");

        RuleFor(x => x.RequesterId)
            .GreaterThan(0).WithMessage("RequesterId must be a positive number")
            .When(x => x.RequesterId.HasValue);

        RuleFor(x => x.RequesteeId)
            .GreaterThan(0).WithMessage("RequesteeId must be a positive number")
            .When(x => x.RequesteeId.HasValue);
    }
}
