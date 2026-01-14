using Serilog;

namespace GROUPFLOW.Common;

/// <summary>
/// Centralized application constants to eliminate magic strings and numbers.
/// All configurable values and repeated literals are defined here.
/// </summary>
public static class AppConstants
{
    // ============================
    // Logging Configuration
    // ============================
    public const string LogsPath = "logs/app-.txt";
    public const RollingInterval LogRollingInterval = RollingInterval.Day;

    // ============================
    // Endpoints
    // ============================
    public const string GraphQLEndpoint = "/api";
    public const string HealthCheckEndpoint = "/health";
    public const string ErrorEndpoint = "/Error";
    public const string HealthCheckName = "Database";

    // ============================
    // Namespaces for Assembly Scanning
    // ============================
    public const string MutationsNamespace = "GROUPFLOW.GraphQL.Mutations";
    public const string QueriesNamespace = "GROUPFLOW.GraphQL.Queries";
    public const string ServicesNamespace = "GROUPFLOW.Services";

    // ============================
    // Environment Variable Names
    // ============================
    public const string PostgresConnString = "POSTGRES_CONN_STRING";
    public const string PostgresConnStringDev = "POSTGRES_CONN_STRING_DEV";
    public const string PostgresConnStringProd = "POSTGRES_CONN_STRING_PROD";
    public const string JwtSecret = "JWT_SECRET";
    public const string CorsOrigins = "CORS_ORIGINS";

    // ============================
    // CORS Configuration
    // ============================
    public const string DevCorsPolicy = "DevCors";
    public const string ProdCorsPolicy = "ProdCors";
    public const string AppCorsPolicy = "AppCorsPolicy";
    public const string DevCorsOrigin = "http://localhost:3000";
    public const string ProdCorsOrigin = "https://groupflows.netlify.app";

    // ============================
    // Error Messages
    // ============================
    public const string PostgresConnNotSet = "POSTGRES_CONN_STRING not set in environment variables.";
    public const string PostgresConnDevNotSet = "POSTGRES_CONN_STRING_DEV not set in environment variables.";
    public const string PostgresConnProdNotSet = "POSTGRES_CONN_STRING_PROD not set in environment variables.";
    public const string JwtSecretNotFound = "JWT_SECRET not found in environment variables.";
    public const string CorsOriginsNotSet = "CORS_ORIGINS must be set in .env for production environment";
    public const string UserNotAuthenticated = "User not authenticated";
    public const string UserNotFound = "User not found";
    public const string ProjectNotFound = "Project not found";
    public const string PostNotFound = "Post not found";
    public const string CommentNotFound = "Comment not found";
    public const string NotAuthorized = "You are not authorized to perform this action";
    public const string NotProjectMember = "You are not a member of this project";
    public const string AlreadyLiked = "Already liked";
    public const string NotLiked = "Not liked";
    public const string AlreadySaved = "Already saved";
    public const string NotSaved = "Not saved";

    // ============================
    // Database Configuration
    // ============================
    public const int MaxRetryCount = 5;
    public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(10);
    public const int MaxBatchSize = 100;
    public const int MinBatchSize = 1;
    public const int CommandTimeoutSeconds = 30;
    public const int DbPoolSize = 100;

    // ============================
    // JWT Configuration
    // ============================
    public const int AccessTokenExpirationMinutes = 60;
    public const int RefreshTokenExpirationDays = 7;
    public const string AccessTokenCookieName = "access_token";
    public const string RefreshTokenCookieName = "refresh_token";

    // ============================
    // Notification Types
    // ============================
    public const string NotificationTypeLike = "POST_LIKE";
    public const string NotificationTypeComment = "POST_COMMENT";
    public const string NotificationTypeFriendRequest = "FRIEND_REQUEST";
    public const string NotificationTypeProjectInvite = "PROJECT_INVITE";

    // ============================
    // User Roles
    // ============================
    public const string RoleUser = "User";
    public const string RoleModerator = "Moderator";
    public const string RoleAdmin = "Admin";

    // ============================
    // Project Roles
    // ============================
    public const string ProjectRoleOwner = "Owner";
    public const string ProjectRoleAdmin = "Admin";
    public const string ProjectRoleCollaborator = "Collaborator";
    public const string ProjectRoleViewer = "Viewer";

    // ============================
    // Pagination Defaults
    // ============================
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
