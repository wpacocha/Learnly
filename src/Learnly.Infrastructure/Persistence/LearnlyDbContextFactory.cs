using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Learnly.Infrastructure.Persistence;

/// <summary>
/// Used by the EF Core CLI (<c>dotnet ef</c>) for design-time operations.
/// Set <c>ConnectionStrings__DefaultConnection</c> or rely on the development fallback below.
/// </summary>
public sealed class LearnlyDbContextFactory : IDesignTimeDbContextFactory<LearnlyDbContext>
{
    public LearnlyDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=learnly;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<LearnlyDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new LearnlyDbContext(optionsBuilder.Options);
    }
}
