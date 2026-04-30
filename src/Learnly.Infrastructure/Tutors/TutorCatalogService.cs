using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Tutors;

public sealed class TutorCatalogService : ITutorCatalogService
{
    private readonly LearnlyDbContext _db;

    public TutorCatalogService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetSubjectsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Subjects.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LookupItemDto>> GetTeachingLevelsAsync(CancellationToken cancellationToken = default)
    {
        return await _db.TeachingLevels.AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new LookupItemDto(x.Id, x.Name))
            .ToListAsync(cancellationToken);
    }
}
