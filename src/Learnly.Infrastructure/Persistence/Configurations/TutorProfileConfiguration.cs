using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorProfileConfiguration : IEntityTypeConfiguration<TutorProfile>
{
    public void Configure(EntityTypeBuilder<TutorProfile> builder)
    {
        builder.ToTable("tutor_profiles");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(e => e.UserId)
            .IsUnique();

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.TeachingLevel)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Location)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Description)
            .HasMaxLength(4000);

        builder.Property(e => e.HourlyRate)
            .HasPrecision(10, 2);

        builder.Property(e => e.PhotoUrl)
            .HasMaxLength(2000);

        builder.Property(e => e.CreatedAtUtc);
        builder.Property(e => e.UpdatedAtUtc);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
