using Learnly.Application.Reviews.Dtos;

namespace Learnly.Application.Reviews;

public interface IReviewService
{
    Task<(ReviewDto? review, string? error)> CreateAsync(
        CreateReviewRequestDto dto,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReviewDto>> GetForTutorAsync(
        Guid tutorProfileId,
        CancellationToken cancellationToken = default);
}
