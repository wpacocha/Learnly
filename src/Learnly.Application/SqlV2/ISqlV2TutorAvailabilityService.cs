using Learnly.Application.SqlV2.Dtos;

namespace Learnly.Application.SqlV2;

public interface ISqlV2TutorAvailabilityService
{
    Task<(SqlTutorAvailabilityDto? slot, string? error)> CreateAsync(CreateTutorAvailabilityRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlTutorAvailabilityDto>> GetForTutorAsync(int tutorId, CancellationToken cancellationToken = default);
}
