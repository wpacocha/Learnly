using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorTeachingOfferingConfiguration : IEntityTypeConfiguration<TutorTeachingOffering>
{
    public void Configure(EntityTypeBuilder<TutorTeachingOffering> builder)
    {
        builder.ToTable("tutor_teaching_offerings");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TeachingMode)
            .HasConversion<int>();

        builder.Property(e => e.Location)
            .HasMaxLength(300);

        builder.Property(e => e.HourlyRate)
            .HasPrecision(10, 2);

        builder.Property(e => e.DurationMinutes)
            .IsRequired();

        builder.HasIndex(e => e.TutorProfileId);
        builder.HasIndex(e => new { e.TutorProfileId, e.SubjectId });

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(e => e.TutorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
