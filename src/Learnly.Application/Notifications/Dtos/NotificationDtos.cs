namespace Learnly.Application.Notifications.Dtos;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Message,
    string? LinkUrl,
    Guid? LessonId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);
