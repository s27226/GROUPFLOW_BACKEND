using NAME_WIP_BACKEND.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.AspNetCore;
using NAME_WIP_BACKEND;
using NAME_WIP_BACKEND.Controllers;
using NAME_WIP_BACKEND.GraphQL.Types;
using NAME_WIP_BACKEND.Services;
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



builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuthMutation>()
    .AddTypeExtension<PostTypeExtensions>()
    .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Types.UserTypeExtensions>()
    .AddTypeExtension<NAME_WIP_BACKEND.GraphQL.Types.ProjectTypeExtensions>()
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
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:3000";
        policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

using var app = builder.Build();

app.UseCors();
app.UseRouting();
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