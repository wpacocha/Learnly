using Learnly.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Learnly.Infrastructure.Persistence.Configurations;

public sealed class DbRoleConfiguration : IEntityTypeConfiguration<DbRole>
{
    public void Configure(EntityTypeBuilder<DbRole> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.RoleId);
        b.Property(x => x.RoleId).HasColumnName("role_id");
        b.Property(x => x.RoleName).HasColumnName("role_name").HasMaxLength(20).IsRequired();
    }
}

public sealed class DbUserConfiguration : IEntityTypeConfiguration<DbUser>
{
    public void Configure(EntityTypeBuilder<DbUser> b)
    {
        b.ToTable("Users");
        b.HasKey(x => x.UserId);
        b.Property(x => x.UserId).HasColumnName("user_id");
        b.Property(x => x.RoleId).HasColumnName("role_id");
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        b.Property(x => x.Surname).HasColumnName("surname").HasMaxLength(100).IsRequired();
        b.Property(x => x.Password).HasColumnName("password").HasMaxLength(255).IsRequired();
        b.HasIndex(x => x.Email).IsUnique();
    }
}

public sealed class DbSubjectConfiguration : IEntityTypeConfiguration<DbSubject>
{
    public void Configure(EntityTypeBuilder<DbSubject> b)
    {
        b.ToTable("Subjects");
        b.HasKey(x => x.SubjectId);
        b.Property(x => x.SubjectId).HasColumnName("subject_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
    }
}

public sealed class DbLevelConfiguration : IEntityTypeConfiguration<DbLevel>
{
    public void Configure(EntityTypeBuilder<DbLevel> b)
    {
        b.ToTable("Levels");
        b.HasKey(x => x.LevelId);
        b.Property(x => x.LevelId).HasColumnName("level_id");
        b.Property(x => x.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
    }
}

public sealed class DbTutorOfferConfiguration : IEntityTypeConfiguration<DbTutorOffer>
{
    public void Configure(EntityTypeBuilder<DbTutorOffer> b)
    {
        b.ToTable("TutorOffers");
        b.HasKey(x => x.TutorOffersId);
        b.Property(x => x.TutorOffersId).HasColumnName("tutoroffers_id");
        b.Property(x => x.TutorId).HasColumnName("tutor_id");
        b.Property(x => x.SubjectId).HasColumnName("subject_id");
        b.Property(x => x.LevelId).HasColumnName("level_id");
        b.Property(x => x.LessonType).HasColumnName("lesson_type").HasMaxLength(10).IsRequired();
        b.Property(x => x.Localization).HasColumnName("localization").HasMaxLength(255);
        b.Property(x => x.HourlyRate).HasColumnName("hourly_rate").HasPrecision(10, 2);
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(x => new { x.TutorId, x.SubjectId, x.LevelId, x.LessonType }).IsUnique();
    }
}

public sealed class DbTutorAvailabilityConfiguration : IEntityTypeConfiguration<DbTutorAvailability>
{
    public void Configure(EntityTypeBuilder<DbTutorAvailability> b)
    {
        b.ToTable("Tutor_availability");
        b.HasKey(x => x.TutorAvailabilityId);
        b.Property(x => x.TutorAvailabilityId).HasColumnName("tutor_availability_id");
        b.Property(x => x.TutorId).HasColumnName("tutor_id");
        b.Property(x => x.StartTime).HasColumnName("start_time");
        b.Property(x => x.EndTime).HasColumnName("end_time");
        b.Property(x => x.Localization).HasColumnName("localization").HasMaxLength(255);
    }
}

public sealed class DbLessonRequestConfiguration : IEntityTypeConfiguration<DbLessonRequest>
{
    public void Configure(EntityTypeBuilder<DbLessonRequest> b)
    {
        b.ToTable("Lesson_requests");
        b.HasKey(x => x.RequestId);
        b.Property(x => x.RequestId).HasColumnName("request_id");
        b.Property(x => x.StudentId).HasColumnName("student_id");
        b.Property(x => x.TutorOffersId).HasColumnName("tutoroffers_id");
        b.Property(x => x.RequestedTimeStart).HasColumnName("requested_time_start");
        b.Property(x => x.RequestedTimeEnd).HasColumnName("requested_time_end");
        b.Property(x => x.Status).HasColumnName("status").HasMaxLength(10);
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(x => new { x.StudentId, x.TutorOffersId, x.RequestedTimeStart }).IsUnique();
    }
}

public sealed class DbLessonConfiguration : IEntityTypeConfiguration<DbLesson>
{
    public void Configure(EntityTypeBuilder<DbLesson> b)
    {
        b.ToTable("Lessons");
        b.HasKey(x => x.LessonId);
        b.Property(x => x.LessonId).HasColumnName("lesson_id");
        b.Property(x => x.RequestId).HasColumnName("request_id");
        b.Property(x => x.Status).HasColumnName("status").HasMaxLength(10);
        b.Property(x => x.MeetingUrl).HasColumnName("meeting_url").HasMaxLength(500);
        b.Property(x => x.IsPaid).HasColumnName("is_paid");
        b.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(x => x.RequestId).IsUnique();
    }
}

public sealed class DbReviewConfiguration : IEntityTypeConfiguration<DbReview>
{
    public void Configure(EntityTypeBuilder<DbReview> b)
    {
        b.ToTable("Reviews");
        b.HasKey(x => x.ReviewId);
        b.Property(x => x.ReviewId).HasColumnName("review_id");
        b.Property(x => x.LessonId).HasColumnName("lesson_id");
        b.Property(x => x.TutorId).HasColumnName("tutor_id");
        b.Property(x => x.StudentId).HasColumnName("student_id");
        b.Property(x => x.Rate).HasColumnName("rate").HasPrecision(3, 1);
        b.Property(x => x.Comment).HasColumnName("comment");
        b.HasIndex(x => x.LessonId).IsUnique();
    }
}

public sealed class DbWhiteboardConfiguration : IEntityTypeConfiguration<DbWhiteboard>
{
    public void Configure(EntityTypeBuilder<DbWhiteboard> b)
    {
        b.ToTable("Whiteboards");
        b.HasKey(x => x.WhiteboardId);
        b.Property(x => x.WhiteboardId).HasColumnName("whiteboard_id");
        b.Property(x => x.LessonId).HasColumnName("lesson_id");
        b.Property(x => x.ImageUrl).HasColumnName("image_url").HasMaxLength(500).IsRequired();
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.HasIndex(x => x.LessonId).IsUnique();
    }
}

public sealed class DbConversationConfiguration : IEntityTypeConfiguration<DbConversation>
{
    public void Configure(EntityTypeBuilder<DbConversation> b)
    {
        b.ToTable("Conversations");
        b.HasKey(x => x.ConversationId);
        b.Property(x => x.ConversationId).HasColumnName("conversation_id");
        b.Property(x => x.TutorId).HasColumnName("tutor_id");
        b.Property(x => x.StudentId).HasColumnName("student_id");
        b.Property(x => x.CreatedAt).HasColumnName("created_at");
        b.HasIndex(x => new { x.TutorId, x.StudentId }).IsUnique();
    }
}

public sealed class DbMessageConfiguration : IEntityTypeConfiguration<DbMessage>
{
    public void Configure(EntityTypeBuilder<DbMessage> b)
    {
        b.ToTable("Messages");
        b.HasKey(x => x.MessageId);
        b.Property(x => x.MessageId).HasColumnName("message_id");
        b.Property(x => x.ConversationId).HasColumnName("conversation_id");
        b.Property(x => x.SenderId).HasColumnName("sender_id");
        b.Property(x => x.WhiteboardId).HasColumnName("whiteboard_id");
        b.Property(x => x.MessageType).HasColumnName("message_type").HasMaxLength(10).IsRequired();
        b.Property(x => x.IsRead).HasColumnName("is_read");
        b.Property(x => x.Content).HasColumnName("content");
        b.Property(x => x.FileUrl).HasColumnName("file_url").HasMaxLength(500);
        b.Property(x => x.SentAt).HasColumnName("sent_at");
    }
}
