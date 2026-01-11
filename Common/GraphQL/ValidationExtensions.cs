using System.ComponentModel.DataAnnotations;
using FluentValidation;
using FluentValidation.Results;
using HotChocolate;
using ValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;

namespace GROUPFLOW.Common.GraphQL;

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

    #region FluentValidation Integration

    /// <summary>
    /// Validates an input object using FluentValidation and throws GraphQL errors if validation fails.
    /// </summary>
    public static void ValidateWithFluent<T>(this T input, IValidator<T> validator) where T : class
    {
        var result = validator.Validate(input);
        if (!result.IsValid)
        {
            throw CreateGraphQLException(result.Errors);
        }
    }

    /// <summary>
    /// Validates an input object using FluentValidation asynchronously.
    /// </summary>
    public static async Task ValidateWithFluentAsync<T>(this T input, IValidator<T> validator, CancellationToken ct = default) where T : class
    {
        var result = await validator.ValidateAsync(input, ct);
        if (!result.IsValid)
        {
            throw CreateGraphQLException(result.Errors);
        }
    }

    /// <summary>
    /// Creates a GraphQL exception from FluentValidation errors.
    /// </summary>
    private static GraphQLException CreateGraphQLException(IEnumerable<ValidationFailure> failures)
    {
        var errors = failures
            .Select(f => new Error(
                f.ErrorMessage,
                "VALIDATION_ERROR",
                extensions: new Dictionary<string, object?>
                {
                    { "field", f.PropertyName },
                    { "attemptedValue", f.AttemptedValue }
                }))
            .ToList();
        
        return new GraphQLException(errors);
    }

    #endregion

    #region Manual Validation Helpers

    /// <summary>
    /// Throws a validation error for a specific field.
    /// </summary>
    public static void ThrowValidationError(string field, string message)
    {
        throw new GraphQLException(new Error(
            message,
            "VALIDATION_ERROR",
            extensions: new Dictionary<string, object?> { { "field", field } }));
    }

    /// <summary>
    /// Throws a validation error if the condition is true.
    /// </summary>
    public static void ThrowIf(bool condition, string field, string message)
    {
        if (condition)
        {
            ThrowValidationError(field, message);
        }
    }

    /// <summary>
    /// Throws a validation error if the value is null or empty.
    /// </summary>
    public static void ThrowIfNullOrEmpty(string? value, string field, string message = "Value is required")
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ThrowValidationError(field, message);
        }
    }

    #endregion
}
