using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class WhiteboardEventConfiguration : IEntityTypeConfiguration<WhiteboardEvent>
{
    public void Configure(EntityTypeBuilder<WhiteboardEvent> builder)
    {
        builder.ToTable("whiteboard_events");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PayloadJson)
            .IsRequired()
            .HasMaxLength(20000);

        builder.HasIndex(x => new { x.LessonId, x.CreatedAtUtc });

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
