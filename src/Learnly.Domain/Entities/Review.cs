namespace Learnly.Domain.Entities;

public sealed class Review
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Guid TutorProfileId { get; set; }
    public string StudentUserId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
}
