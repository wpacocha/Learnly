namespace Learnly.Application.SqlV2.Dtos;

public sealed record SqlLookupDto(int Id, string Name);

public sealed record SqlTutorOfferDto(
    int TutorOffersId,
    int TutorId,
    int SubjectId,
    int LevelId,
    string LessonType,
    string? Localization,
    decimal HourlyRate,
    DateTime UpdatedAt);

public sealed record SqlTutorAvailabilityDto(
    int TutorAvailabilityId,
    int TutorId,
    DateTime StartTime,
    DateTime EndTime,
    string? Localization);

public sealed record SqlLessonRequestDto(
    int RequestId,
    int StudentId,
    int TutorOffersId,
    DateTime RequestedTimeStart,
    DateTime RequestedTimeEnd,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record SqlLessonDto(
    int LessonId,
    int RequestId,
    string Status,
    string? MeetingUrl,
    bool IsPaid,
    DateTime UpdatedAt);
