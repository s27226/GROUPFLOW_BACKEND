using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Results;
using HotChocolate;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace NAME_WIP_BACKEND.GraphQL;

/// <summary>
/// Extension methods for input validation.
/// Supports both DataAnnotations and FluentValidation.
/// </summary>
public static class ValidationExtensions
{
    #region DataAnnotations Validation (Legacy support)

    /// <summary>
    /// Validates an input object using DataAnnotations and throws GraphQL errors if validation fails.
    /// Prefer using FluentValidation validators for new code.
    /// </summary>
    public static void ValidateInput<T>(this T input) where T : class
    {
        var validationResults = new List<ValidationResult>();
        var context = new ValidationContext(input);
        
        if (!Validator.TryValidateObject(input, context, validationResults, validateAllProperties: true))
        {
            var errors = validationResults
                .Select(vr => new Error(
                    vr.ErrorMessage ?? "Validation failed",
                    "VALIDATION_ERROR",
                    extensions: new Dictionary<string, object?>
                    {
                        { "field", string.Join(", ", vr.MemberNames) }
                    }))
                .ToList();
            
            throw new GraphQLException(errors);
        }
    }
    
    /// <summary>
    /// Validates an input object and returns whether it's valid.
    /// </summary>
    public static bool TryValidateInput<T>(this T input, out List<ValidationResult> validationResults) where T : class
    {
        validationResults = new List<ValidationResult>();
        var context = new ValidationContext(input);
        return Validator.TryValidateObject(input, context, validationResults, validateAllProperties: true);
    }

    #endregion

    #region FluentValidation

    /// <summary>
    /// Validates the input using FluentValidation and throws a GraphQLException if validation fails.
    /// </summary>
    /// <typeparam name="T">The type of input to validate</typeparam>
    /// <param name="input">The input to validate</param>
    /// <param name="validator">The FluentValidation validator</param>
    /// <exception cref="GraphQLException">Thrown when validation fails</exception>
    public static void ValidateWith<T>(this T input, IValidator<T> validator)
    {
        var result = validator.Validate(input);
        if (!result.IsValid)
        {
            throw CreateGraphQLException(result.Errors);
        }
    }

    /// <summary>
    /// Validates the input asynchronously using FluentValidation and throws a GraphQLException if validation fails.
    /// </summary>
    public static async Task ValidateWithAsync<T>(this T input, IValidator<T> validator, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(input, cancellationToken);
        if (!result.IsValid)
        {
            throw CreateGraphQLException(result.Errors);
        }
    }

    /// <summary>
    /// Tries to get a validator from the service provider and validates the input.
    /// Falls back to DataAnnotations if no FluentValidation validator is registered.
    /// </summary>
    public static void Validate<T>(this T input, IServiceProvider services) where T : class
    {
        var validator = services.GetService<IValidator<T>>();
        if (validator != null)
        {
            input.ValidateWith(validator);
        }
        else
        {
            // Fall back to DataAnnotations
            input.ValidateInput();
        }
    }

    private static GraphQLException CreateGraphQLException(IEnumerable<ValidationFailure> failures)
    {
        var errors = failures.Select(f => new Error(
            f.ErrorMessage,
            code: "VALIDATION_ERROR",
            extensions: new Dictionary<string, object?>
            {
                ["field"] = f.PropertyName,
                ["attemptedValue"] = f.AttemptedValue
            }
        )).ToList();

        return new GraphQLException(errors);
    }

    #endregion
}
