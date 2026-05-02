namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorSearchResultDto(
    Guid TutorProfileId,
    string FirstName,
    string LastName,
    decimal MinHourlyRate,
    decimal MaxHourlyRate,
    string? PhotoUrl,
    IReadOnlyList<int> SubjectIds,
    IReadOnlyList<int> TeachingLevelIds,
    IReadOnlyList<TutorOfferingPublicDto> Offerings,
    IReadOnlyList<TutorAvailabilitySlotDto> AvailableSlots);
