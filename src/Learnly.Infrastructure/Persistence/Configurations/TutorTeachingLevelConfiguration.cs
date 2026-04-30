using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorTeachingLevelConfiguration : IEntityTypeConfiguration<TutorTeachingLevel>
{
    public void Configure(EntityTypeBuilder<TutorTeachingLevel> builder)
    {
        builder.ToTable("tutor_teaching_levels");

        builder.HasKey(e => new { e.TutorProfileId, e.TeachingLevelId });

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(e => e.TutorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TeachingLevel>()
            .WithMany()
            .HasForeignKey(e => e.TeachingLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.TeachingLevelId);
    }
}
