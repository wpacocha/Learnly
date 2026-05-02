using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Learnly.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core CLI (<c>dotnet ef</c>) for design-time operations.
/// Set <c>ConnectionStrings__DefaultConnection</c> or rely on Learnly.Api/appsettings.json next to this solution.
/// </summary>
public sealed class LearnlyDbContextFactory : IDesignTimeDbContextFactory<LearnlyDbContext>
{
    public LearnlyDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<LearnlyDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LearnlyDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        // .../Learnly.Infrastructure/bin/Debug/net8.0 → .../src/Learnly.Api
        var apiDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Learnly.Api"));
        var appsettings = Path.Combine(apiDir, "appsettings.json");
        if (File.Exists(appsettings))
        {
            var cfg = new ConfigurationBuilder()
                .SetBasePath(apiDir)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            var cs = cfg.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(cs))
            {
                return cs;
            }
        }

        // Last resort (matches repo appsettings.json when using default local Postgres)
        return "Host=localhost;Port=5432;Database=learnly;Username=postgres;Password=3006";
    }
}
