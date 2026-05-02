using Learnly.Domain.Entities;

namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorOfferingPublicDto(
    int SubjectId,
    TeachingMode TeachingMode,
    string? Location,
    decimal HourlyRate,
    int DurationMinutes,
    IReadOnlyList<int> TeachingLevelIds);
