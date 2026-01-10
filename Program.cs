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



var builder = WebApplication.CreateBuilder(args);
DotNetEnv.Env.Load();

// Sprawdzenie środowiska
var env = builder.Environment;

// Wybór connection stringa w zależności od środowiska
string connectionString = env.IsDevelopment()
    ? Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING_DEV")
      ?? throw new InvalidOperationException("POSTGRES_CONN_STRING_DEV not set")
    : Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING_PROD")
      ?? throw new InvalidOperationException("POSTGRES_CONN_STRING_PROD not set");

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
            npgsqlOptions.MinBatchSize(1);
            npgsqlOptions.MaxBatchSize(100);
            npgsqlOptions.CommandTimeout(30);
        }));
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
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = env.IsDevelopment())
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
    options.AddPolicy("DevCors",policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ORIGINS") ?? "http://localhost:3000";
        policy.WithOrigins(allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries))
              .AllowCredentials()
              .AllowAnyHeader() // Allow all headers for cookie-based auth
              .WithMethods("GET", "POST", "OPTIONS");
    });
    
    options.AddPolicy("ProdCors",policy =>
    {
        policy.WithOrigins("https://groupflows.netlify.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
    
});

using var app = builder.Build();

// Middleware zależne od środowiska
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(env.IsDevelopment() ? "DevCors" : "ProdCors");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL("/api");
app.MapControllers();
// app.Run();

//
// 



 using var scope = app.Services.CreateScope();
 var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 if (env.IsDevelopment())
 {
     DataInitializer.Seed(db);
 }

 //db.Database.Migrate();



app.Run();