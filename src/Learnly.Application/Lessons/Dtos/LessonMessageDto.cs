namespace Learnly.Application.Lessons.Dtos;

public sealed record LessonMessageDto(
    Guid Id,
    Guid LessonId,
    string SenderUserId,
    string Message,
    DateTimeOffset SentAtUtc);
