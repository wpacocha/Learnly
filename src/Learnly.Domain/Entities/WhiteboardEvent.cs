namespace Learnly.Domain.Entities;

public sealed class WhiteboardEvent
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
}
