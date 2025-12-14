using NAME_WIP_BACKEND.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using HotChocolate;
using HotChocolate.AspNetCore;
using NAME_WIP_BACKEND;
using NAME_WIP_BACKEND.Controllers;


DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING")));


builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddTypeExtension<AuthMutation>()
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
        policy.WithOrigins("http://localhost:3000")
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



 using var scope = app.Services.CreateScope();
 var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
DataInitializer.Seed(db);
 //db.Database.Migrate();
foreach (var user in db.Users)
{
    Console.WriteLine($"{user.Id}: {user.Name}, {user.Surname}, {user.Nickname}, {user.Email}, {user.Password}");
}



app.Run();