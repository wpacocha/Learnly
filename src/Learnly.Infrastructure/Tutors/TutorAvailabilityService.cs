using Learnly.Application.Abstractions;
using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Tutors;

public sealed class TutorAvailabilityService : ITutorAvailabilityService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public TutorAvailabilityService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TutorAvailabilitySlotDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return Array.Empty<TutorAvailabilitySlotDto>();
        }

        return await _db.TutorAvailabilitySlots
            .AsNoTracking()
            .Where(x => x.TutorProfileId == profile.Id)
            .OrderBy(x => x.StartUtc)
            .Join(
                _db.TutorTeachingOfferings.AsNoTracking(),
                s => s.TutorTeachingOfferingId,
                o => o.Id,
                (s, o) => new TutorAvailabilitySlotDto(s.Id, s.TutorTeachingOfferingId, o.SubjectId, s.StartUtc, s.EndUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<(TutorAvailabilitySlotDto? slot, string? error)> CreateMineAsync(
        TutorAvailabilitySlotCreateDto dto,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return (null, "Create tutor profile first.");
        }

        var offering = await _db.TutorTeachingOfferings
            .FirstOrDefaultAsync(
                x => x.Id == dto.TutorTeachingOfferingId && x.TutorProfileId == profile.Id,
                cancellationToken);
        if (offering is null)
        {
            return (null, "Nie znaleziono wybranej pozycji (przedmiot w profilu).");
        }

        if (offering.DurationMinutes < 15)
        {
            return (null, "Nieprawidłowa długość lekcji w profilu.");
        }

        var endUtc = dto.StartUtc.AddMinutes(offering.DurationMinutes);
        if (endUtc <= dto.StartUtc)
        {
            return (null, "Nie udało się wyliczyć końca slotu.");
        }

        if (dto.StartUtc <= DateTimeOffset.UtcNow)
        {
            return (null, "Availability slot must start in the future.");
        }

        var overlaps = await _db.TutorAvailabilitySlots.AnyAsync(x =>
                x.TutorProfileId == profile.Id
                && dto.StartUtc < x.EndUtc
                && endUtc > x.StartUtc,
            cancellationToken);

        if (overlaps)
        {
            return (null, "The availability slot overlaps with an existing slot.");
        }

        var slot = new TutorAvailabilitySlot
        {
            Id = Guid.NewGuid(),
            TutorProfileId = profile.Id,
            TutorTeachingOfferingId = offering.Id,
            StartUtc = dto.StartUtc,
            EndUtc = endUtc
        };

        _db.TutorAvailabilitySlots.Add(slot);
        await _db.SaveChangesAsync(cancellationToken);

        return (new TutorAvailabilitySlotDto(slot.Id, slot.TutorTeachingOfferingId, offering.SubjectId, slot.StartUtc, slot.EndUtc), null);
    }

    public async Task<string?> DeleteMineAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return "Create tutor profile first.";
        }

        var slot = await _db.TutorAvailabilitySlots
            .FirstOrDefaultAsync(x => x.Id == slotId && x.TutorProfileId == profile.Id, cancellationToken);

        if (slot is null)
        {
            return "Availability slot not found.";
        }

        _db.TutorAvailabilitySlots.Remove(slot);
        await _db.SaveChangesAsync(cancellationToken);
        return null;
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
