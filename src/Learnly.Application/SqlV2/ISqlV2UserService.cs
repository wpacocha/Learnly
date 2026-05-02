using Learnly.Application.SqlV2.Dtos;

namespace Learnly.Application.SqlV2;

public interface ISqlV2UserService
{
    Task<IReadOnlyList<SqlUserDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<(SqlUserDto? user, string? error)> CreateUserAsync(CreateSqlUserRequest request, CancellationToken cancellationToken = default);
}
