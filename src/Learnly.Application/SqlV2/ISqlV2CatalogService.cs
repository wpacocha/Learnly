using Learnly.Application.SqlV2.Dtos;

namespace Learnly.Application.SqlV2;

public interface ISqlV2CatalogService
{
    Task<IReadOnlyList<SqlLookupDto>> GetSubjectsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlLookupDto>> GetLevelsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SqlLookupDto>> GetTutorsAsync(CancellationToken cancellationToken = default);
}
