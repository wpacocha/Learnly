namespace Learnly.Domain.Entities;

public sealed class LessonMessage
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; set; }
}
