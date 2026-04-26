namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorProfileDto(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    string Subject,
    string TeachingLevel,
    string Location,
    string? Description,
    decimal HourlyRate,
    string? PhotoUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
