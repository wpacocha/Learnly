namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorSearchResultDto(
    Guid TutorProfileId,
    string Headline,
    string Location,
    decimal HourlyRate,
    string? PhotoUrl,
    IReadOnlyList<int> SubjectIds,
    IReadOnlyList<int> TeachingLevelIds,
    IReadOnlyList<TutorAvailabilitySlotDto> AvailableSlots);
