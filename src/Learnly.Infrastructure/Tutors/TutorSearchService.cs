using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Tutors;

public sealed class TutorSearchService : ITutorSearchService
{
    private readonly LearnlyDbContext _db;

    public TutorSearchService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<TutorSearchResultDto>> SearchAsync(
        TutorSearchQueryDto query,
        CancellationToken cancellationToken = default)
    {
        var tutorProfilesQuery = _db.TutorProfiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Location))
        {
            var location = query.Location.Trim().ToLower();
            tutorProfilesQuery = tutorProfilesQuery.Where(p => p.Location.ToLower().Contains(location));
        }

        if (query.SubjectId.HasValue)
        {
            var tutorIdsForSubject = _db.TutorSubjects
                .Where(ts => ts.SubjectId == query.SubjectId.Value)
                .Select(ts => ts.TutorProfileId);
            tutorProfilesQuery = tutorProfilesQuery.Where(p => tutorIdsForSubject.Contains(p.Id));
        }

        if (query.TeachingLevelId.HasValue)
        {
            var tutorIdsForLevel = _db.TutorTeachingLevels
                .Where(tl => tl.TeachingLevelId == query.TeachingLevelId.Value)
                .Select(tl => tl.TutorProfileId);
            tutorProfilesQuery = tutorProfilesQuery.Where(p => tutorIdsForLevel.Contains(p.Id));
        }

        var profiles = await tutorProfilesQuery
            .OrderBy(p => p.HourlyRate)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
        {
            return Array.Empty<TutorSearchResultDto>();
        }

        var profileIds = profiles.Select(p => p.Id).ToArray();

        var subjects = await _db.TutorSubjects
            .Where(x => profileIds.Contains(x.TutorProfileId))
            .ToListAsync(cancellationToken);

        var levels = await _db.TutorTeachingLevels
            .Where(x => profileIds.Contains(x.TutorProfileId))
            .ToListAsync(cancellationToken);

        var slotsQuery = _db.TutorAvailabilitySlots
            .Where(x => profileIds.Contains(x.TutorProfileId));

        if (query.AvailableFromUtc.HasValue)
        {
            slotsQuery = slotsQuery.Where(x => x.StartUtc >= query.AvailableFromUtc.Value);
        }

        if (query.AvailableToUtc.HasValue)
        {
            slotsQuery = slotsQuery.Where(x => x.EndUtc <= query.AvailableToUtc.Value);
        }

        var slots = await slotsQuery
            .OrderBy(x => x.StartUtc)
            .ToListAsync(cancellationToken);

        var result = profiles.Select(profile =>
        {
            var profileSubjects = subjects.Where(x => x.TutorProfileId == profile.Id).Select(x => x.SubjectId).Distinct().OrderBy(x => x).ToList();
            var profileLevels = levels.Where(x => x.TutorProfileId == profile.Id).Select(x => x.TeachingLevelId).Distinct().OrderBy(x => x).ToList();
            var profileSlots = slots.Where(x => x.TutorProfileId == profile.Id)
                .Select(x => new TutorAvailabilitySlotDto(x.Id, x.StartUtc, x.EndUtc))
                .ToList();

            return new TutorSearchResultDto(
                profile.Id,
                profile.Headline,
                profile.Location,
                profile.HourlyRate,
                profile.PhotoUrl,
                profileSubjects,
                profileLevels,
                profileSlots);
        }).ToList();

        return result;
    }
}
