namespace NAME_WIP_BACKEND.GraphQL.Responses;

/// <summary>
/// Response DTO for authentication payloads.
/// </summary>
public record AuthPayloadResponse(
    string Token,
    UserResponse User,
    string? RefreshToken = null
);

/// <summary>
/// Response DTO for token refresh operations.
/// </summary>
public record TokenRefreshResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
