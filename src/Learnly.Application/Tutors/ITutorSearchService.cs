using Learnly.Application.Tutors.Dtos;

namespace Learnly.Application.Tutors;

public interface ITutorSearchService
{
    Task<IReadOnlyList<TutorSearchResultDto>> SearchAsync(
        TutorSearchQueryDto query,
        CancellationToken cancellationToken = default);
}
