using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorConversationConfiguration : IEntityTypeConfiguration<TutorConversation>
{
    public void Configure(EntityTypeBuilder<TutorConversation> builder)
    {
        builder.ToTable("tutor_conversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StudentUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasIndex(x => new { x.TutorProfileId, x.StudentUserId })
            .IsUnique();

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(x => x.TutorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
