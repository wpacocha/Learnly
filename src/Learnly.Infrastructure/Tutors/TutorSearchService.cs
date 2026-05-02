using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Learnly.Domain.Entities;
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
        var offeringsQuery = _db.TutorTeachingOfferings.AsNoTracking().AsQueryable();

        if (query.SubjectId.HasValue)
        {
            offeringsQuery = offeringsQuery.Where(o => o.SubjectId == query.SubjectId.Value);
        }

        if (query.TeachingLevelId.HasValue)
        {
            var lid = query.TeachingLevelId.Value;
            offeringsQuery = offeringsQuery.Where(o =>
                o.OfferingLevels.Any(l => l.TeachingLevelId == lid));
        }

        if (query.TeachingMode.HasValue)
        {
            var tm = query.TeachingMode.Value;
            if (tm == TeachingMode.Online)
            {
                offeringsQuery = offeringsQuery.Where(o =>
                    o.TeachingMode == TeachingMode.Online || o.TeachingMode == TeachingMode.Both);
            }
            else if (tm == TeachingMode.Stationary)
            {
                offeringsQuery = offeringsQuery.Where(o =>
                    o.TeachingMode == TeachingMode.Stationary || o.TeachingMode == TeachingMode.Both);
            }
            else
            {
                offeringsQuery = offeringsQuery.Where(o => o.TeachingMode == TeachingMode.Both);
            }
        }

        var loc = query.Location?.Trim();
        var useLocation = !string.IsNullOrEmpty(loc)
            && (!query.TeachingMode.HasValue
                || query.TeachingMode == TeachingMode.Stationary
                || query.TeachingMode == TeachingMode.Both);
        if (useLocation)
        {
            var ll = loc!.ToLower();
            offeringsQuery = offeringsQuery.Where(o =>
                (o.TeachingMode == TeachingMode.Stationary || o.TeachingMode == TeachingMode.Both)
                && o.Location != null
                && o.Location.ToLower().Contains(ll));
        }

        var matchingTutorIds = await offeringsQuery
            .Select(o => o.TutorProfileId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (matchingTutorIds.Count == 0)
        {
            return Array.Empty<TutorSearchResultDto>();
        }

        var profiles = await _db.TutorProfiles
            .AsNoTracking()
            .Where(p => matchingTutorIds.Contains(p.Id))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Take(100)
            .ToListAsync(cancellationToken);

        if (profiles.Count == 0)
        {
            return Array.Empty<TutorSearchResultDto>();
        }

        var profileIds = profiles.Select(p => p.Id).ToArray();

        var allOfferings = await _db.TutorTeachingOfferings
            .AsNoTracking()
            .Include(o => o.OfferingLevels)
            .Where(o => profileIds.Contains(o.TutorProfileId))
            .ToListAsync(cancellationToken);

        var slotsQuery = _db.TutorAvailabilitySlots
            .AsNoTracking()
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

        var result = new List<TutorSearchResultDto>();
        foreach (var profile in profiles)
        {
            var pOfferings = allOfferings.Where(x => x.TutorProfileId == profile.Id).ToList();
            if (pOfferings.Count == 0)
            {
                continue;
            }

            var rates = pOfferings.Select(x => x.HourlyRate).ToList();
            var minRate = rates.Min();
            var maxRate = rates.Max();

            var subjectIds = pOfferings.Select(x => x.SubjectId).Distinct().OrderBy(x => x).ToList();
            var levelIds = pOfferings
                .SelectMany(x => x.OfferingLevels.Select(l => l.TeachingLevelId))
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var publicOfferings = pOfferings
                .Select(o => new TutorOfferingPublicDto(
                    o.SubjectId,
                    o.TeachingMode,
                    o.Location,
                    o.HourlyRate,
                    o.DurationMinutes,
                    o.OfferingLevels.Select(l => l.TeachingLevelId).Distinct().OrderBy(x => x).ToList()))
                .ToList();

            var profileSlots = slots.Where(x => x.TutorProfileId == profile.Id).ToList();
            var offeringById = pOfferings.ToDictionary(x => x.Id);
            var slotDtos = profileSlots
                .Where(s => offeringById.ContainsKey(s.TutorTeachingOfferingId))
                .Select(s => new TutorAvailabilitySlotDto(
                    s.Id,
                    s.TutorTeachingOfferingId,
                    offeringById[s.TutorTeachingOfferingId].SubjectId,
                    s.StartUtc,
                    s.EndUtc))
                .ToList();

            result.Add(new TutorSearchResultDto(
                profile.Id,
                profile.FirstName,
                profile.LastName,
                minRate,
                maxRate,
                profile.PhotoUrl,
                subjectIds,
                levelIds,
                publicOfferings,
                slotDtos));
        }

        return result.OrderBy(x => x.MinHourlyRate).ToList();
    }
}
