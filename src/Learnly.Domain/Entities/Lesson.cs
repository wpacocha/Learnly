namespace Learnly.Domain.Entities;

public sealed class Lesson
{
    public Guid Id { get; set; }
    public string StudentUserId { get; set; } = string.Empty;
    public Guid TutorProfileId { get; set; }
    public Guid TutorAvailabilitySlotId { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
    public LessonStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
