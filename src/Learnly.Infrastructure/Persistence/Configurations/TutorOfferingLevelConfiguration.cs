using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorOfferingLevelConfiguration : IEntityTypeConfiguration<TutorOfferingLevel>
{
    public void Configure(EntityTypeBuilder<TutorOfferingLevel> builder)
    {
        builder.ToTable("tutor_offering_levels");

        builder.HasKey(e => new { e.TutorTeachingOfferingId, e.TeachingLevelId });

        builder.HasOne(e => e.TutorTeachingOffering)
            .WithMany(o => o.OfferingLevels)
            .HasForeignKey(e => e.TutorTeachingOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TeachingLevel>()
            .WithMany()
            .HasForeignKey(e => e.TeachingLevelId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
