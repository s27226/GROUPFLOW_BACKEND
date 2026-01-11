using System.Security.Claims;
using NAME_WIP_BACKEND.Exceptions;

namespace NAME_WIP_BACKEND.GraphQL;

/// <summary>
/// Shared helper methods for GraphQL resolvers.
/// Reduces code duplication across mutations and queries.
/// </summary>
public static class GraphQLHelpers
{
    /// <summary>
    /// Extracts the current user ID from the ClaimsPrincipal.
    /// Throws AuthenticationException if the user is not authenticated.
    /// </summary>
    public static int GetAuthenticatedUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim))
            throw new AuthenticationException();
        
        return int.Parse(claim);
    }

    /// <summary>
    /// Extracts the current user ID from IHttpContextAccessor.
    /// Throws AuthenticationException if the user is not authenticated.
    /// </summary>
    public static int GetAuthenticatedUserId(this IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user == null)
            throw new AuthenticationException();
        
        return user.GetAuthenticatedUserId();
    }

    /// <summary>
    /// Tries to get the authenticated user ID without throwing.
    /// Returns null if the user is not authenticated.
    /// </summary>
    public static int? TryGetAuthenticatedUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
            return null;
        
        return userId;
    }

    /// <summary>
    /// Tries to get the authenticated user ID from IHttpContextAccessor without throwing.
    /// </summary>
    public static int? TryGetAuthenticatedUserId(this IHttpContextAccessor httpContextAccessor)
    {
        return httpContextAccessor.HttpContext?.User.TryGetAuthenticatedUserId();
    }
}
