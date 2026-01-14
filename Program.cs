using System.Reflection;
using GROUPFLOW.Common;
using GROUPFLOW.Common.Database;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using GROUPFLOW.Common.GraphQL;
using GROUPFLOW.Features.Auth.GraphQL;
using GROUPFLOW.Features.Posts.GraphQL;
using GROUPFLOW.Features.Posts.Services;
using GROUPFLOW.Features.Blobs.Services;
using GROUPFLOW.Features.Friendships.Services;
using GROUPFLOW.Features.Notifications.Services;
using GROUPFLOW.Features.Projects.Services;
using Amazon.S3;

// Load environment variables from .env file
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

    // Configure port - use PORT env var for AWS EB compatibility, default to 5000
    var port = GetEnv("PORT", "5000");
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    // Environment detection
    var env = builder.Environment;
    var isDev = env.IsDevelopment();

    // Helper functions for environment variables
    static string RequireEnv(string name) =>
        Environment.GetEnvironmentVariable(name)
        ?? throw new InvalidOperationException($"Required environment variable '{name}' is not set");

    static string GetEnv(string name, string defaultValue = "") =>
        Environment.GetEnvironmentVariable(name) ?? defaultValue;

    static int GetEnvInt(string name, int defaultValue) =>
        int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;

    static bool GetEnvBool(string name, bool defaultValue) =>
        bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;

    // Database configuration
    var connectionString = RequireEnv(AppConstants.PostgresConnString);
    var maxRetryCount = GetEnvInt("DB_MAX_RETRY_COUNT", AppConstants.MaxRetryCount);
    var maxRetryDelaySeconds = GetEnvInt("DB_MAX_RETRY_DELAY_SECONDS", (int)AppConstants.MaxRetryDelay.TotalSeconds);
    var commandTimeoutSeconds = GetEnvInt("DB_COMMAND_TIMEOUT_SECONDS", AppConstants.CommandTimeoutSeconds);
    var enableSensitiveDataLogging = GetEnvBool("DB_SENSITIVE_LOGGING", isDev);

    // DbContext Pool configuration
    builder.Services.AddDbContextPool<AppDbContext>(options =>
    {
        options.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: maxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelaySeconds),
                    errorCodesToAdd: null);
                npgsqlOptions.MinBatchSize(AppConstants.MinBatchSize);
                npgsqlOptions.MaxBatchSize(AppConstants.MaxBatchSize);
                npgsqlOptions.CommandTimeout(commandTimeoutSeconds);
            });
        if (enableSensitiveDataLogging)
        {
            options.EnableSensitiveDataLogging();
        }
    });

    // Health checks
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>(AppConstants.HealthCheckName);

    // Register services with constructor injection
    // Core services
    builder.Services.AddScoped<GROUPFLOW.Common.Data.DataInitializer>();
    builder.Services.AddScoped<GROUPFLOW.Features.Projects.Services.ProjectService>();
    builder.Services.AddScoped<GROUPFLOW.Features.Notifications.Services.NotificationService>();
    builder.Services.AddScoped<GROUPFLOW.Features.Blobs.Services.IS3Service, GROUPFLOW.Features.Blobs.Services.S3Service>();
    
    // Feature services
    builder.Services.AddScoped<IFriendshipService, FriendshipService>();
    builder.Services.AddScoped<IPostService, PostService>();

    // GraphQL Mutation classes (for constructor injection)
    builder.Services.AddScoped<GROUPFLOW.Features.Projects.GraphQL.Mutations.ProjectMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Chat.GraphQL.Mutations.EntryMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Friendships.GraphQL.Mutations.FriendRequestMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Friendships.GraphQL.Mutations.FriendshipMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Projects.GraphQL.Mutations.ProjectInvitationMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Projects.GraphQL.Mutations.ProjectRecommendationMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Projects.GraphQL.Mutations.ProjectEventMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Posts.GraphQL.Mutations.SavedPostMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Users.GraphQL.Mutations.UserTagMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Users.GraphQL.Mutations.UserMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Posts.GraphQL.Mutations.PostMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Notifications.GraphQL.Mutations.NotificationMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Friendships.GraphQL.Mutations.BlockedUserMutation>();
    builder.Services.AddScoped<GROUPFLOW.Features.Moderation.GraphQL.Mutations.ModerationMutation>();

    // AWS S3 service (if configured)
    var awsAccessKey = GetEnv("AWS_ACCESS_KEY_ID");
    if (!string.IsNullOrEmpty(awsAccessKey))
    {
        builder.Services.AddAWSService<IAmazonS3>();
    }

    // GraphQL configuration
    var includeExceptionDetails = GetEnvBool("GRAPHQL_INCLUDE_EXCEPTION_DETAILS", isDev);
    var disableIntrospection = GetEnvBool("GRAPHQL_DISABLE_INTROSPECTION", !isDev);

    // Register GraphQL error filter for unified error handling
    builder.Services.AddSingleton<GraphQLErrorFilter>();

    builder.Services
        .AddGraphQLServer()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddTypeExtension<GROUPFLOW.Features.Auth.GraphQL.Mutations.AuthMutation>()
        .AddTypeExtension<GROUPFLOW.Features.Posts.GraphQL.Extensions.PostTypeExtensions>()
        .AddTypeExtension<GROUPFLOW.Features.Users.GraphQL.Extensions.UserTypeExtensions>()
        .AddTypeExtension<GROUPFLOW.Features.Projects.GraphQL.Extensions.ProjectTypeExtensions>()
        .AddTypeExtension<GROUPFLOW.Features.Blobs.GraphQL.Extensions.BlobFileTypeExtensions>()
        .AddTypeExtension<GROUPFLOW.Features.Blobs.GraphQL.Mutations.BlobMutation>()
        .AddTypeExtension<GROUPFLOW.Features.Blobs.GraphQL.Queries.BlobQuery>()
        .AddErrorFilter<GraphQLErrorFilter>()
        .AddAuthorization()
        .AddProjections()
        .AddFiltering()
        .AddSorting()
        .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = includeExceptionDetails)
        .DisableIntrospection(disableIntrospection);

    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();

    // JWT Authentication
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(options =>
    {
        var jwtSecret = RequireEnv(AppConstants.JwtSecret);
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtSecret))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Skip authentication for refresh token requests
                if (context.Request.ContentType?.Contains("application/json") == true)
                {
                    context.Request.EnableBuffering();
                    using var reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                    var body = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    context.Request.Body.Position = 0;

                    if (body.Contains("refreshToken", StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.CompletedTask;
                    }
                }

                // Read token from cookie first, then header
                if (context.Request.Cookies.TryGetValue(AppConstants.AccessTokenCookieName, out var cookieToken))
                {
                    context.Token = cookieToken;
                }
                else if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    var headerValue = authHeader.ToString();
                    if (headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        context.Token = headerValue["Bearer ".Length..].Trim();
                    }
                }

                return Task.CompletedTask;
            }
        };
    });

    // CORS configuration
    builder.Services.AddCors(options =>
    {
        var corsOrigins = GetEnv(AppConstants.CorsOrigins, isDev ? AppConstants.DevCorsOrigin : "");
        var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (origins.Length == 0 && !isDev)
        {
            throw new InvalidOperationException(AppConstants.CorsOriginsNotSet);
        }

        options.AddPolicy(AppConstants.AppCorsPolicy, policy =>
        {
            policy.WithOrigins(origins)
                  .AllowCredentials()
                  .AllowAnyHeader()
                  .WithMethods("GET", "POST", "OPTIONS");
        });
    });

    var app = builder.Build();

    // Middleware pipeline
    if (isDev)
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler(AppConstants.ErrorEndpoint);
        app.UseHsts();
        // Note: HTTPS redirection removed for EB deployment - load balancer handles SSL termination
    }

    app.UseCors(AppConstants.AppCorsPolicy);
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGraphQL(AppConstants.GraphQLEndpoint);
    app.MapControllers();
    app.MapHealthChecks("/health");

    // Start the app first so health checks can respond
    // Run database initialization in background to avoid blocking startup
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    _ = Task.Run(async () =>
    {
        try
        {
            await InitializeDatabaseAsync(app, isDev);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Database initialization failed - application may not function correctly");
        }
    });

    logger.LogInformation("Application started successfully");
    await app.RunAsync();

/// <summary>
/// Initializes the database: applies migrations and seeds data in development.
/// Uses async operations to prevent blocking.
/// </summary>
static async Task InitializeDatabaseAsync(WebApplication app, bool isDev)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<AppDbContext>();

    try
    {
        logger.LogInformation("Applying database migrations...");
        await db.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully");

        // Only seed in development
        if (isDev)
        {
            var initializer = services.GetRequiredService<GROUPFLOW.Common.Data.DataInitializer>();
            
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            await initializer.SeedAsync(cts.Token);
        }
    }
    catch (OperationCanceledException)
    {
        logger.LogError("Database initialization was cancelled (timeout)");
        throw;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database initialization");
        throw;
    }
}

// Marker class for assembly reference
public partial class Program { }