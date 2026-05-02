using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews_v2");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Rating).IsRequired();
        builder.Property(x => x.Comment).HasMaxLength(2000);

        builder.HasIndex(x => x.LessonId).IsUnique();
        builder.HasIndex(x => x.TutorProfileId);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(x => x.TutorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
