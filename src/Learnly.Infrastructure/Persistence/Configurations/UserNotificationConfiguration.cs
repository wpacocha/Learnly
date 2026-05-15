using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("user_notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.LinkUrl)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.UserId, x.ReadAtUtc });
        builder.HasIndex(x => new { x.Type, x.LessonId, x.UserId })
            .IsUnique()
            .HasFilter("\"LessonId\" IS NOT NULL");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
