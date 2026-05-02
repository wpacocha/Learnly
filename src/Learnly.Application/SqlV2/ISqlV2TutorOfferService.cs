using Learnly.Application.SqlV2.Dtos;

namespace Learnly.Application.SqlV2;

public interface ISqlV2TutorOfferService
{
    Task<(SqlTutorOfferDto? offer, string? error)> CreateAsync(CreateTutorOfferRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlTutorOfferDto>> SearchAsync(int? subjectId, int? levelId, string? lessonType, string? localization, CancellationToken cancellationToken = default);
}
