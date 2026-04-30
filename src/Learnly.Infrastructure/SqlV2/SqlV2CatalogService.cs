using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.SqlV2;

public sealed class SqlV2CatalogService : ISqlV2CatalogService
{
    private readonly LearnlyDbContext _db;

    public SqlV2CatalogService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<SqlLookupDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.DbSubjects
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SqlLookupDto(x.SubjectId, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SqlLookupDto>> GetLevelsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.DbLevels
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SqlLookupDto(x.LevelId, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SqlLookupDto>> GetTutorsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.DbUsers
            .AsNoTracking()
            .Where(x => x.RoleId == 3) // schema default: tutor role_id
            .OrderBy(x => x.Surname)
            .ThenBy(x => x.Name)
            .Select(x => new SqlLookupDto(x.UserId, $"{x.Name} {x.Surname}"))
            .ToListAsync(cancellationToken);
    }
}
