using System.Reflection;
using NAME_WIP_BACKEND.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.AspNetCore;
using NAME_WIP_BACKEND;
using NAME_WIP_BACKEND.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrutor;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using NAME_WIP_BACKEND.Services;
using Serilog;

DotNetEnv.Env.Load();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


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
        }));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

builder.Services.AddAWSService<IAmazonS3>();

var assembly = Assembly.GetExecutingAssembly();

// Rejestracja wszystkich serwisów kończących się na "Service"
builder.Services.Scan(scan => scan
    .FromAssemblies(assembly)
    .AddClasses(classes => classes
        .Where(type => 
            type.Name.EndsWith("Service")
            ))
    .AsSelf()
    .AsImplementedInterfaces()
    .WithScopedLifetime()
);


// Rejestracja wszystkich mutacji kończących się na "Mutation"
builder.Services.Scan(scan => scan
    .FromAssemblies(assembly)
    .AddClasses(classes => classes
        .Where(t =>
            t.Name.EndsWith("Mutation") &&
            t.Namespace == "NAME_WIP_BACKEND.GraphQL.Mutations"))
    .AsSelf()
    .WithScopedLifetime()
);

builder.Services.AddScoped<AuthMutation>();

// builder.Services.AddScoped<Mutation>();
// builder.Services
//     .AddGraphQLServer()
//     .AddQueryType<Query>()
//     .AddMutationType<Mutation>()
//     .AddTypeExtension<AuthMutation>()
//     .AddAuthorization()
//     .AddProjections()
//     .AddFiltering()
//     .AddSorting()
//     .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = env.IsDevelopment())
//     .DisableIntrospection(false);

builder.Host.UseSerilog();
builder.Services.AddControllers();
// builder.Services.AddHttpContextAccessor();
// builder.Services.AddAuthentication(options =>
// {
//     options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//     options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
// }).AddJwtBearer(options =>
// {
//     options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//     {
//         ValidateIssuer = false,
//         ValidateAudience = false,
//         ValidateLifetime = true,
//         ValidateIssuerSigningKey = true,
//         IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException("JWT_SECRET not found in environment variables.")))
//     };
// });

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors",policy =>
    {
        // policy.AllowAnyOrigin()
        //     .AllowAnyHeader()
        //     .AllowAnyMethod();
        
        
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              ;
    });
    
    options.AddPolicy("ProdCors",policy =>
    {
        policy.WithOrigins("https://groupflows.netlify.app")
            .AllowAnyHeader()
            .AllowAnyMethod()
            ;
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
// app.UseAuthentication();
// app.UseAuthorization();

// app.MapGraphQL("/api").RequireCors(env.IsDevelopment() ? "DevCors" : "ProdCors");;
app.MapControllers().RequireCors(env.IsDevelopment() ? "DevCors" : "ProdCors");
app.MapHealthChecks("/health");
// app.Run();

//
// 



 using var scope = app.Services.CreateScope();
 var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataInitializer>>();
if (false) // env.IsDevelopment()
 {
    await DataInitializer.Seed(db, logger);
}


app.Run();

Log.CloseAndFlush();