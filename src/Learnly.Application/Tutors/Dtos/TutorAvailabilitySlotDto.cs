namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorAvailabilitySlotDto(
    Guid Id,
    Guid TutorTeachingOfferingId,
    int SubjectId,
    DateTimeOffset StartUtc,
    DateTimeOffset EndUtc);
