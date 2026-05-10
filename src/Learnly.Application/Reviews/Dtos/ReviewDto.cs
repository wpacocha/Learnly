namespace Learnly.Application.Reviews.Dtos;

public sealed record ReviewDto(
    Guid Id,
    Guid LessonId,
    Guid TutorProfileId,
    string StudentUserId,
    double Rating,
    string? Comment,
    DateTimeOffset CreatedAtUtc);
