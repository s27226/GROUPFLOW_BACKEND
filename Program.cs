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


DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING")));

// Configure AWS S3 Client
builder.Services.AddSingleton<IAmazonS3>(sp =>
{
    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID") ?? throw new InvalidOperationException("AWS_ACCESS_KEY_ID not found");
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY") ?? throw new InvalidOperationException("AWS_SECRET_ACCESS_KEY not found");
    var region = Environment.GetEnvironmentVariable("AWS_REGION") ?? "us-east-1";

    var credentials = new BasicAWSCredentials(accessKey, secretKey);
    var config = new AmazonS3Config
    {
        RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(region)
    };

    return new AmazonS3Client(credentials, config);
});

builder.Services.AddSingleton<IS3Service, S3Service>();

// Register business logic services - isolate DB from API
builder.Services.AddScoped<IFriendshipService, FriendshipService>();
builder.Services.AddScoped<IPostService, PostService>();



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
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .DisableIntrospection(false);

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
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:3000";
        policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowCredentials()
              .AllowAnyHeader() // Allow all headers for cookie-based auth
              .WithMethods("GET", "POST", "OPTIONS");
    });
});

using var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/api");
app.MapControllers();
// app.Run();

//
// 



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
        
        foreach (var user in db.Users)
        {
            Console.WriteLine($"{user.Id}: {user.Name}, {user.Surname}, {user.Nickname}, {user.Email}, {user.Password}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error during database initialization: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
}



app.Run();