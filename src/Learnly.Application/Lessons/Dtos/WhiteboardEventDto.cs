namespace Learnly.Application.Lessons.Dtos;

public sealed record WhiteboardEventDto(
    Guid Id,
    Guid LessonId,
    string SenderUserId,
    string EventType,
    string PayloadJson,
    DateTimeOffset CreatedAtUtc);
