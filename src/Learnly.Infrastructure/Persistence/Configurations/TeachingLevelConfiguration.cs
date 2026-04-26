using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TeachingLevelConfiguration : IEntityTypeConfiguration<TeachingLevel>
{
    public void Configure(EntityTypeBuilder<TeachingLevel> builder)
    {
        builder.ToTable("teaching_levels");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
