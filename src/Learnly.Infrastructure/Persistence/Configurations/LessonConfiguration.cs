using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("lessons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => x.TutorAvailabilitySlotId)
            .IsUnique();

        builder.HasIndex(x => x.StudentUserId);
        builder.HasIndex(x => x.TutorProfileId);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(x => x.TutorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TutorAvailabilitySlot>()
            .WithMany()
            .HasForeignKey(x => x.TutorAvailabilitySlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
