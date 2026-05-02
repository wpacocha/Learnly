namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorProfileDto(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    string Bio,
    string? PhotoUrl,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc);
