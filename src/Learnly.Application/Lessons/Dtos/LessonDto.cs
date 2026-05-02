using Learnly.Domain.Entities;

namespace Learnly.Application.Lessons.Dtos;

public sealed record LessonDto(
    Guid Id,
    string StudentUserId,
    Guid TutorProfileId,
    Guid TutorAvailabilitySlotId,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc,
    LessonStatus Status,
    DateTimeOffset CreatedAtUtc);
