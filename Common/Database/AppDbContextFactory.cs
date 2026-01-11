using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GROUPFLOW.Common.Database;

/// <summary>
/// Factory for creating AppDbContext during design-time (migrations, etc.)
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        DotNetEnv.Env.Load();
        
        var connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONN_STRING")
            ?? throw new InvalidOperationException("POSTGRES_CONN_STRING not found in environment variables.");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
