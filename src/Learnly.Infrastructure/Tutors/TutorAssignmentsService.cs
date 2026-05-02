using Learnly.Application.Abstractions;
using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Tutors;

public sealed class TutorAssignmentsService : ITutorAssignmentsService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public TutorAssignmentsService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TutorTeachingOfferingDto>?> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var rows = await _db.TutorTeachingOfferings
            .AsNoTracking()
            .Include(o => o.OfferingLevels)
            .Where(x => x.TutorProfileId == profile.Id)
            .OrderBy(x => x.SubjectId)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return rows.Select(MapRow).ToList();
    }

    public async Task<(IReadOnlyList<TutorTeachingOfferingDto>? data, string? error)> UpsertMineAsync(
        TutorOfferingsUpsertDto dto,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return (null, "Create tutor profile first.");
        }

        foreach (var o in dto.Offerings)
        {
            if (o.TeachingMode is TeachingMode.Stationary or TeachingMode.Both)
            {
                if (string.IsNullOrWhiteSpace(o.Location))
                {
                    return (null, "Podaj lokalizację dla trybu stacjonarnego lub hybrydowego.");
                }
            }
        }

        var subjectIds = dto.Offerings.Select(x => x.SubjectId).Distinct().ToArray();
        var validSubjects = await _db.Subjects.Where(s => subjectIds.Contains(s.Id)).Select(s => s.Id).ToArrayAsync(cancellationToken);
        if (validSubjects.Length != subjectIds.Length)
        {
            return (null, "One or more subject IDs are invalid.");
        }

        var allLevelIds = dto.Offerings.SelectMany(x => x.TeachingLevelIds).Distinct().ToArray();
        var validLevels = await _db.TeachingLevels.Where(l => allLevelIds.Contains(l.Id)).Select(l => l.Id).ToArrayAsync(cancellationToken);
        if (validLevels.Length != allLevelIds.Length)
        {
            return (null, "One or more teaching level IDs are invalid.");
        }

        var existing = await _db.TutorTeachingOfferings
            .Include(x => x.OfferingLevels)
            .Where(x => x.TutorProfileId == profile.Id)
            .ToListAsync(cancellationToken);

        var existingById = existing.ToDictionary(e => e.Id);
        var incomingIds = dto.Offerings.Where(x => x.Id.HasValue).Select(x => x.Id!.Value).ToHashSet();
        foreach (var o in dto.Offerings.Where(x => x.Id.HasValue))
        {
            if (!existingById.ContainsKey(o.Id!.Value))
            {
                return (null, "Nie znaleziono pozycji oferty do aktualizacji.");
            }
        }

        var toRemoveIds = existing.Where(e => !incomingIds.Contains(e.Id)).Select(e => e.Id).ToList();
        if (toRemoveIds.Count > 0)
        {
            var slotsToDrop = await _db.TutorAvailabilitySlots
                .Where(s => toRemoveIds.Contains(s.TutorTeachingOfferingId))
                .ToListAsync(cancellationToken);
            _db.TutorAvailabilitySlots.RemoveRange(slotsToDrop);
            foreach (var rid in toRemoveIds)
            {
                _db.TutorTeachingOfferings.Remove(existingById[rid]);
                existingById.Remove(rid);
            }
        }

        foreach (var o in dto.Offerings)
        {
            var levels = o.TeachingLevelIds.Distinct().OrderBy(x => x).ToArray();
            if (levels.Length == 0)
            {
                return (null, "Każda pozycja musi mieć co najmniej jeden poziom nauczania.");
            }

            if (o.Id.HasValue)
            {
                var entity = existingById[o.Id.Value];
                entity.SubjectId = o.SubjectId;
                entity.TeachingMode = o.TeachingMode;
                entity.Location = string.IsNullOrWhiteSpace(o.Location) ? null : o.Location.Trim();
                entity.HourlyRate = o.HourlyRate;
                entity.DurationMinutes = o.DurationMinutes;
                entity.OfferingLevels.Clear();
                foreach (var lid in levels)
                {
                    entity.OfferingLevels.Add(new TutorOfferingLevel
                    {
                        TutorTeachingOffering = entity,
                        TeachingLevelId = lid,
                    });
                }
            }
            else
            {
                var entity = new TutorTeachingOffering
                {
                    Id = Guid.NewGuid(),
                    TutorProfileId = profile.Id,
                    SubjectId = o.SubjectId,
                    TeachingMode = o.TeachingMode,
                    Location = string.IsNullOrWhiteSpace(o.Location) ? null : o.Location.Trim(),
                    HourlyRate = o.HourlyRate,
                    DurationMinutes = o.DurationMinutes,
                };
                foreach (var lid in levels)
                {
                    entity.OfferingLevels.Add(new TutorOfferingLevel
                    {
                        TutorTeachingOffering = entity,
                        TeachingLevelId = lid,
                    });
                }

                _db.TutorTeachingOfferings.Add(entity);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        var list = await _db.TutorTeachingOfferings
            .AsNoTracking()
            .Include(x => x.OfferingLevels)
            .Where(x => x.TutorProfileId == profile.Id)
            .OrderBy(x => x.SubjectId)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return (list.Select(MapRow).ToList(), null);
    }

    private static TutorTeachingOfferingDto MapRow(TutorTeachingOffering o)
    {
        var levelIds = o.OfferingLevels.Select(x => x.TeachingLevelId).Distinct().OrderBy(x => x).ToList();
        return new TutorTeachingOfferingDto(
            o.Id,
            o.SubjectId,
            o.TeachingMode,
            o.Location,
            o.HourlyRate,
            o.DurationMinutes,
            levelIds);
    }

    private async Task<TutorProfile?> GetMyProfileAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return null;
        }

        return await _db.TutorProfiles.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }
}
