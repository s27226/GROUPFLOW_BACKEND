using Serilog;

namespace NAME_WIP_BACKEND;

public static class AppConstants
{
    // Ścieżki i nazwy
    public const string LogsPath = "logs/log-.txt";
    public const string HealthCheckName = "Database";
    public const string MutationsNamespace = "NAME_WIP_BACKEND.GraphQL.Mutations";
    public const string GraphQLEndpoint = "/api";
    public const string HealthEndpoint = "/health";
    public const string ErrorEndpoint = "/Error";
    
    // Zmienne środowiskowe
    public const string PostgresConnStringDev = "POSTGRES_CONN_STRING_DEV";
    public const string PostgresConnStringProd = "POSTGRES_CONN_STRING_PROD";
    public const string JwtSecret = "JWT_SECRET";
    
    // Origins CORS
    public const string DevCorsOrigin = "http://localhost:3000";
    public const string ProdCorsOrigin = "https://groupflows.netlify.app";
    
    // Nazwy polityk CORS
    public const string DevCorsPolicy = "DevCors";
    public const string ProdCorsPolicy = "ProdCors";
    
    // Wiadomości błędów
    public const string PostgresConnDevNotSet = "POSTGRES_CONN_STRING_DEV not set";
    public const string PostgresConnProdNotSet = "POSTGRES_CONN_STRING_PROD not set";
    public const string JwtSecretNotFound = "JWT_SECRET not found in environment variables.";
    
    // Wartości liczbowe
    public const int MaxRetryCount = 5;
    public static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(10);
    public const RollingInterval LogRollingInterval = RollingInterval.Day;
}