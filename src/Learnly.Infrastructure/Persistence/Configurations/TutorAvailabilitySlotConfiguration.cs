using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorAvailabilitySlotConfiguration : IEntityTypeConfiguration<TutorAvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<TutorAvailabilitySlot> builder)
    {
        builder.ToTable("tutor_availability_slots");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.StartUtc).IsRequired();
        builder.Property(e => e.EndUtc).IsRequired();

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(e => e.TutorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TutorTeachingOffering>()
            .WithMany()
            .HasForeignKey(e => e.TutorTeachingOfferingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.TutorProfileId, e.StartUtc, e.EndUtc });
    }
}
