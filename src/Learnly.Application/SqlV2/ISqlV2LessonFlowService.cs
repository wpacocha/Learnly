using Learnly.Application.SqlV2.Dtos;

namespace Learnly.Application.SqlV2;

public interface ISqlV2LessonFlowService
{
    Task<(SqlLessonRequestDto? request, string? error)> CreateRequestAsync(CreateLessonRequestRequest request, CancellationToken cancellationToken = default);
    Task<(SqlLessonRequestDto? request, string? error)> ChangeRequestStatusAsync(int requestId, ChangeLessonRequestStatusRequest request, CancellationToken cancellationToken = default);
    Task<(SqlLessonDto? lesson, string? error)> CreateLessonAsync(CreateLessonFromRequestRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlLessonRequestDto>> GetRequestsForTutorAsync(int tutorId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlLessonDto>> GetLessonsForUserAsync(int userId, CancellationToken cancellationToken = default);
}
