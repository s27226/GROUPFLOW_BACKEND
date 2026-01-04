using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DotNetEnv;
using NAME_WIP_BACKEND.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        Env.Load();

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                  ?? "Development";

        string connectionString = env == "Development"
            ? Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING_DEV")
            : Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING_PROD");


        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString ?? throw new InvalidOperationException("Connection string not found in environment variables."));

        return new AppDbContext(optionsBuilder.Options);
    }
}
