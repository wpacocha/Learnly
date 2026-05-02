using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class TutorSubjectConfiguration : IEntityTypeConfiguration<TutorSubject>
{
    public void Configure(EntityTypeBuilder<TutorSubject> builder)
    {
        builder.ToTable("tutor_subjects");

        builder.HasKey(e => new { e.TutorProfileId, e.SubjectId });

        builder.HasOne<TutorProfile>()
            .WithMany()
            .HasForeignKey(e => e.TutorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Subject>()
            .WithMany()
            .HasForeignKey(e => e.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.SubjectId);
    }
}
