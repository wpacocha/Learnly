using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Persistence;

public sealed class LearnlyDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    public LearnlyDbContext(DbContextOptions<LearnlyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<TeachingLevel> TeachingLevels => Set<TeachingLevel>();
    public DbSet<TutorProfile> TutorProfiles => Set<TutorProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearnlyDbContext).Assembly);
    }
}
