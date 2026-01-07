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
    .WriteTo.File(AppConstants.LogsPath, rollingInterval: AppConstants.LogRollingInterval)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);


// Sprawdzenie środowiska
var env = builder.Environment;

// Wybór connection stringa w zależności od środowiska
string connectionString = env.IsDevelopment()
    ? Environment.GetEnvironmentVariable(AppConstants.PostgresConnStringDev)
      ?? throw new InvalidOperationException(AppConstants.PostgresConnDevNotSet)
    : Environment.GetEnvironmentVariable(AppConstants.PostgresConnStringProd)
      ?? throw new InvalidOperationException(AppConstants.PostgresConnProdNotSet);

builder.Services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(maxRetryCount: AppConstants.MaxRetryCount, maxRetryDelay: AppConstants.MaxRetryDelay, errorCodesToAdd: null);
        }));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(AppConstants.HealthCheckName);

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
            t.Namespace == AppConstants.MutationsNamespace))
    .AsSelf()
    .WithScopedLifetime()
);

builder.Services.AddScoped<AuthMutation>();

builder.Services.AddScoped<Mutation>();
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuthMutation>()
    .AddAuthorization()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = env.IsDevelopment())
    .DisableIntrospection(false);

builder.Host.UseSerilog();
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
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(AppConstants.JwtSecret) ?? throw new InvalidOperationException(AppConstants.JwtSecretNotFound)))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(AppConstants.DevCorsPolicy,policy =>
    {
        // policy.AllowAnyOrigin()
        //     .AllowAnyHeader()
        //     .AllowAnyMethod();
        
        
        policy.WithOrigins(AppConstants.DevCorsOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              ;
    });
    
    options.AddPolicy(AppConstants.ProdCorsPolicy,policy =>
    {
        policy.WithOrigins(AppConstants.ProdCorsOrigin)
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
    app.UseExceptionHandler(AppConstants.ErrorEndpoint);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors(env.IsDevelopment() ? AppConstants.DevCorsPolicy : AppConstants.ProdCorsPolicy);
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGraphQL(AppConstants.GraphQLEndpoint).RequireCors(env.IsDevelopment() ? AppConstants.DevCorsPolicy : AppConstants.ProdCorsPolicy);;
app.MapControllers().RequireCors(env.IsDevelopment() ? AppConstants.DevCorsPolicy : AppConstants.ProdCorsPolicy);
app.MapHealthChecks(AppConstants.HealthCheckName);
// app.Run();

//
// 



 using var scope = app.Services.CreateScope();
 var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
 var logger = scope.ServiceProvider.GetRequiredService<ILogger<DataInitializer>>();
if (env.IsDevelopment())
 {
    await DataInitializer.Seed(db, logger);
}


app.Run();

Log.CloseAndFlush();