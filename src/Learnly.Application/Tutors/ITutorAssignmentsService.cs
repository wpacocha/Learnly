using Learnly.Application.Tutors.Dtos;

namespace Learnly.Application.Tutors;

public interface ITutorAssignmentsService
{
    Task<IReadOnlyList<TutorTeachingOfferingDto>?> GetMineAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<TutorTeachingOfferingDto>? data, string? error)> UpsertMineAsync(
        TutorOfferingsUpsertDto dto,
        CancellationToken cancellationToken = default);
}
