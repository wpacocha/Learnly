namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorAvailabilitySlotDto(
    Guid Id,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc);
