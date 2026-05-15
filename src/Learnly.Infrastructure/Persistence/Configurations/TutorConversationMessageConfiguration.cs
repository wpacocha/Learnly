using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorConversationMessageConfiguration : IEntityTypeConfiguration<TutorConversationMessage>
{
    public void Configure(EntityTypeBuilder<TutorConversationMessage> builder)
    {
        builder.ToTable("tutor_conversation_messages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SenderUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Message)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(x => new { x.ConversationId, x.SentAtUtc });

        builder.HasOne<TutorConversation>()
            .WithMany()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.SenderUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
