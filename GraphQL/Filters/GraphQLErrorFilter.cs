using HotChocolate;
using GROUPFLOW.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace GROUPFLOW.GraphQL.Filters;

/// <summary>
/// Unified error filter that converts domain exceptions to GraphQL errors.
/// This provides a consistent error handling pattern across all mutations and queries.
/// </summary>
public class GraphQLErrorFilter : IErrorFilter
{
    private readonly ILogger<GraphQLErrorFilter> _logger;
    private readonly IHostEnvironment _environment;

    public GraphQLErrorFilter(ILogger<GraphQLErrorFilter> logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public IError OnError(IError error)
    {
        var exception = error.Exception;
        
        if (exception == null)
            return error;

        return exception switch
        {
            AuthenticationException authEx => CreateError(error, authEx.Message, "AUTHENTICATION_ERROR", 401),
            AuthorizationException authzEx => CreateError(error, authzEx.Message, "AUTHORIZATION_ERROR", 403),
            EntityNotFoundException notFoundEx => CreateError(error, notFoundEx.Message, "NOT_FOUND", 404, 
                new Dictionary<string, object?>
                {
                    ["entityType"] = notFoundEx.EntityType,
                    ["entityId"] = notFoundEx.EntityId
                }),
            DuplicateEntityException dupEx => CreateError(error, dupEx.Message, "DUPLICATE_ENTITY", 409,
                new Dictionary<string, object?>
                {
                    ["entityType"] = dupEx.EntityType,
                    ["field"] = dupEx.Field
                }),
            ValidationException valEx => CreateValidationError(error, valEx),
            BusinessRuleException bizEx => CreateError(error, bizEx.Message, "BUSINESS_RULE_VIOLATION", 400),
            OperationCanceledException => CreateError(error, "The operation was cancelled", "OPERATION_CANCELLED", 499),
            _ => HandleUnexpectedException(error, exception)
        };
    }

    private IError CreateError(IError originalError, string message, string code, int statusCode, 
        Dictionary<string, object?>? extensions = null)
    {
        var builder = ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .SetExtension("statusCode", statusCode);

        if (extensions != null)
        {
            foreach (var kvp in extensions)
            {
                builder.SetExtension(kvp.Key, kvp.Value);
            }
        }

        return builder.Build();
    }

    private IError CreateValidationError(IError originalError, ValidationException valEx)
    {
        return ErrorBuilder.New()
            .SetMessage(valEx.Message)
            .SetCode("VALIDATION_ERROR")
            .SetExtension("statusCode", 400)
            .SetExtension("errors", valEx.Errors)
            .Build();
    }

    private IError HandleUnexpectedException(IError error, Exception exception)
    {
        // Log unexpected exceptions
        _logger.LogError(exception, "Unhandled exception in GraphQL request: {Message}", exception.Message);

        // In development, show the actual error message for debugging
        if (_environment.IsDevelopment())
        {
            return ErrorBuilder.New()
                .SetMessage($"{exception.GetType().Name}: {exception.Message}")
                .SetCode("INTERNAL_ERROR")
                .SetExtension("statusCode", 500)
                .SetExtension("stackTrace", exception.StackTrace)
                .SetExtension("exceptionType", exception.GetType().FullName)
                .Build();
        }

        // In production, don't expose internal exception details
        return ErrorBuilder.New()
            .SetMessage("An unexpected error occurred. Please try again later.")
            .SetCode("INTERNAL_ERROR")
            .SetExtension("statusCode", 500)
            .Build();
    }
}
