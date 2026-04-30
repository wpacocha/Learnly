using Learnly.Application.Tutors.Dtos;

namespace Learnly.Application.Tutors;

public interface ITutorCatalogService
{
    Task<IReadOnlyList<LookupItemDto>> GetSubjectsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetTeachingLevelsAsync(CancellationToken cancellationToken = default);
}
