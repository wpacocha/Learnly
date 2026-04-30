namespace Learnly.Application.Tutors.Dtos;

public sealed record TutorSubjectsAndLevelsDto(
    IReadOnlyList<int> SubjectIds,
    IReadOnlyList<int> TeachingLevelIds);
