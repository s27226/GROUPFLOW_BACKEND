using System.Reflection;
using NAME_WIP_BACKEND.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using NAME_WIP_BACKEND;
using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.GraphQL.Types;
using NAME_WIP_BACKEND.Services;
using NAME_WIP_BACKEND.Services.Friendship;
using NAME_WIP_BACKEND.Services.Post;
using Amazon.S3;
using Serilog;

// Load environment variables from .env file
DotNetEnv.Env.Load();

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        AppConstants.LogsPath,
        rollingInterval: AppConstants.LogRollingInterval,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for all logging
    builder.Host.UseSerilog();

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
            })
        .UseLoggerFactory(LoggerFactory.Create(lb => lb.AddSerilog()));
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
    builder.Services.AddScoped<DataInitializer>();
    builder.Services.AddScoped<ProjectService>();
    builder.Services.AddScoped<NotificationService>();
    builder.Services.AddScoped<IS3Service, S3Service>();
    
    // Legacy services (existing interfaces)
    builder.Services.AddScoped<IFriendshipService, FriendshipService>();
    builder.Services.AddScoped<IPostService, PostService>();

    // GraphQL Mutation classes (for constructor injection)
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.ProjectMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.EntryMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.FriendRequestMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.FriendshipMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.ProjectInvitationMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.ProjectRecommendationMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.ProjectEventMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.SavedPostMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.UserTagMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.PostMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.NotificationMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.BlockedUserMutation>();
    builder.Services.AddScoped<NAME_WIP_BACKEND.GraphQL.Mutations.ModerationMutation>();

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
    builder.Services.AddSingleton<NAME_WIP_BACKEND.GraphQL.Filters.GraphQLErrorFilter>();

    builder.Services
        .AddGraphQLServer()
        .AddQueryType<Query>()
        .AddMutationType<Mutation>()
        .AddTypeExtension<AuthMutation>()
        .AddTypeExtension<PostTypeExtensions>()
        .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Types.UserTypeExtensions>()
        .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Types.ProjectTypeExtensions>()
        .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Types.BlobFileTypeExtensions>()
        .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Mutations.BlobMutation>()
        .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Queries.BlobQuery>()
        .AddErrorFilter<NAME_WIP_BACKEND.GraphQL.Filters.GraphQLErrorFilter>()
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
        app.UseHttpsRedirection();
    }

    // Request logging
    app.UseSerilogRequestLogging();

    app.UseCors(AppConstants.AppCorsPolicy);
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGraphQL(AppConstants.GraphQLEndpoint);
    app.MapControllers();
    app.MapHealthChecks(AppConstants.HealthCheckEndpoint);

    // Database initialization (async, with proper cancellation support)
    await InitializeDatabaseAsync(app, isDev);

    Log.Information("Application started successfully");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

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
            var initializer = services.GetRequiredService<DataInitializer>();
            
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