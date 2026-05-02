using Learnly.Domain.Entities;

namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorTeachingOfferingDto(
    Guid Id,
    int SubjectId,
    TeachingMode TeachingMode,
    string? Location,
    decimal HourlyRate,
    int DurationMinutes,
    IReadOnlyList<int> TeachingLevelIds);
