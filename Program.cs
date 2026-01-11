using NAME_WIP_BACKEND.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.AspNetCore;
using NAME_WIP_BACKEND;
using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.GraphQL.Types;
using NAME_WIP_BACKEND.Services;
using NAME_WIP_BACKEND.Services.Friendship;
using NAME_WIP_BACKEND.Services.Post;
using Amazon.S3;
using Amazon.Runtime;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Load .env file - primary source of configuration
DotNetEnv.Env.Load();

// Helper to get required env variable
static string RequireEnv(string name) =>
    Environment.GetEnvironmentVariable(name)
    ?? throw new InvalidOperationException($"Required environment variable '{name}' is not set");

// Helper to get optional env variable with default
static string GetEnv(string name, string defaultValue = "") =>
    Environment.GetEnvironmentVariable(name) ?? defaultValue;

static int GetEnvInt(string name, int defaultValue) =>
    int.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;

static bool GetEnvBool(string name, bool defaultValue) =>
    bool.TryParse(Environment.GetEnvironmentVariable(name), out var value) ? value : defaultValue;

// Environment detection
var env = builder.Environment;
var isDev = env.IsDevelopment();

// Database configuration from .env
var connectionString = RequireEnv("POSTGRES_CONN_STRING");
var maxRetryCount = GetEnvInt("DB_MAX_RETRY_COUNT", 5);
var maxRetryDelaySeconds = GetEnvInt("DB_MAX_RETRY_DELAY_SECONDS", 10);
var commandTimeoutSeconds = GetEnvInt("DB_COMMAND_TIMEOUT_SECONDS", 30);
var enableSensitiveDataLogging = GetEnvBool("DB_SENSITIVE_LOGGING", isDev);

// DbContext Pool - correctly configured for connection pooling
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
            npgsqlOptions.MinBatchSize(1);
            npgsqlOptions.MaxBatchSize(100);
            npgsqlOptions.CommandTimeout(commandTimeoutSeconds);
        });

    if (enableSensitiveDataLogging)
    {
        options.EnableSensitiveDataLogging();
    }
});

// Register FluentValidation validators from assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register business logic services
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IPostService, PostService>();

// GraphQL configuration from .env
var includeExceptionDetails = GetEnvBool("GRAPHQL_INCLUDE_EXCEPTION_DETAILS", isDev);
var disableIntrospection = GetEnvBool("GRAPHQL_DISABLE_INTROSPECTION", !isDev);

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
    .AddAuthorization()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = includeExceptionDetails)
    .DisableIntrospection(disableIntrospection);

builder.Logging.SetMinimumLevel(LogLevel.Debug);
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not found in environment variables.")))
    };

    // Configure JWT to read from cookies instead of Authorization header
    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Skip authentication for refresh token requests - they handle their own validation
            if (context.Request.ContentType?.Contains("application/json") == true)
            {
                context.Request.EnableBuffering();
                using (var reader = new StreamReader(context.Request.Body, System.Text.Encoding.UTF8, leaveOpen: true))
                {
                    var body = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    context.Request.Body.Position = 0;
                    
                    if (body.Contains("refreshToken", StringComparison.OrdinalIgnoreCase))
                    {
                        // Don't set a token for refresh requests - let the mutation handle it
                        return Task.CompletedTask;
                    }
                }
            }

            // For all other requests, use access_token cookie
            if (context.Request.Cookies.ContainsKey("access_token"))
            {
                context.Token = context.Request.Cookies["access_token"];
            }
            // Fall back to Authorization header if cookie is not present
            else if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = authHeader.Substring("Bearer ".Length).Trim();
                }
            }
            
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddCors(options =>
{
    // CORS origins from .env (comma-separated list)
    var corsOrigins = GetEnv("CORS_ORIGINS", isDev ? "http://localhost:3000" : "");
    var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    if (origins.Length == 0 && !isDev)
    {
        throw new InvalidOperationException("CORS_ORIGINS must be set in .env for production environment");
    }

    options.AddPolicy("AppCorsPolicy", policy =>
    {
        policy.WithOrigins(origins)
              .AllowCredentials()
              .AllowAnyHeader()
              .WithMethods("GET", "POST", "OPTIONS");
    });
});

using var app = builder.Build();

// Middleware dependent on environment
if (isDev)
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Use unified CORS policy (configured per environment via appsettings)
app.UseCors("AppCorsPolicy");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/api");
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        // Ensure database is created and all migrations are applied
        Console.WriteLine("Applying database migrations...");
        db.Database.Migrate();
        Console.WriteLine("Migrations applied successfully.");
        
        // Seed initial data
        DataInitializer.Seed(db);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
}

app.Run();