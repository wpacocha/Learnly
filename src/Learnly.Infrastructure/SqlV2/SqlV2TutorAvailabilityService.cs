using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.SqlV2;

public sealed class SqlV2TutorAvailabilityService : ISqlV2TutorAvailabilityService
{
    private readonly LearnlyDbContext _db;

    public SqlV2TutorAvailabilityService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<(SqlTutorAvailabilityDto? slot, string? error)> CreateAsync(
        CreateTutorAvailabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.StartTime >= request.EndTime)
        {
            return (null, "start_time must be before end_time.");
        }

        var tutorExists = await _db.DbUsers.AnyAsync(x => x.UserId == request.TutorId, cancellationToken);
        if (!tutorExists)
        {
            return (null, "Tutor does not exist.");
        }

        var entity = new DbTutorAvailability
        {
            TutorId = request.TutorId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            Localization = request.Localization
        };

        _db.DbTutorAvailabilities.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(entity), null);
    }

    public async Task<IReadOnlyList<SqlTutorAvailabilityDto>> GetForTutorAsync(int tutorId, CancellationToken cancellationToken = default)
    {
        return await _db.DbTutorAvailabilities
            .AsNoTracking()
            .Where(x => x.TutorId == tutorId)
            .OrderBy(x => x.StartTime)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    private static SqlTutorAvailabilityDto Map(DbTutorAvailability x) =>
        new(x.TutorAvailabilityId, x.TutorId, x.StartTime, x.EndTime, x.Localization);
}
