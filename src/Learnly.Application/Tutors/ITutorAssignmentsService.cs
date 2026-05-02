using Learnly.Application.Tutors.Dtos;

namespace Learnly.Application.Tutors;

public interface ITutorAssignmentsService
{
    Task<TutorSubjectsAndLevelsDto?> GetMineAsync(CancellationToken cancellationToken = default);

    Task<(TutorSubjectsAndLevelsDto? data, string? error)> UpsertMineAsync(
        TutorSubjectsAndLevelsUpsertDto dto,
        CancellationToken cancellationToken = default);
}
