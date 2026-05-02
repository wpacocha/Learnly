using Learnly.Application.Lessons.Dtos;
using Learnly.Domain.Entities;

namespace Learnly.Application.Lessons;

public interface ILessonService
{
    Task<(LessonDto? lesson, string? error)> BookAsync(BookLessonRequestDto dto, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LessonDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<(LessonDto? lesson, string? error)> ChangeStatusAsync(
        Guid lessonId,
        LessonStatus status,
        CancellationToken cancellationToken = default);
}
