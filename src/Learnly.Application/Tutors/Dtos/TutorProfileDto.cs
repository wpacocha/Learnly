namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorProfileDto(
    Guid Id,
    string UserId,
    string Headline,
    string Bio,
    string Location,
    decimal HourlyRate,
    string? PhotoUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
