using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class LessonMessageConfiguration : IEntityTypeConfiguration<LessonMessage>
{
    public void Configure(EntityTypeBuilder<LessonMessage> builder)
    {
        builder.ToTable("lesson_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(x => new { x.LessonId, x.SentAtUtc });

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(x => x.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
