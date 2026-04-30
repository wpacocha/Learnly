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
            .Where(x => x.TutorProfileId == profile.Id)
            .OrderBy(x => x.StartUtc)
            .Select(x => new TutorAvailabilitySlotDto(x.Id, x.StartUtc, x.EndUtc))
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

        if (dto.EndUtc <= dto.StartUtc)
        {
            return (null, "EndUtc must be greater than StartUtc.");
        }

        if (dto.StartUtc <= DateTimeOffset.UtcNow)
        {
            return (null, "Availability slot must start in the future.");
        }

        var overlaps = await _db.TutorAvailabilitySlots.AnyAsync(x =>
            x.TutorProfileId == profile.Id
            && dto.StartUtc < x.EndUtc
            && dto.EndUtc > x.StartUtc,
            cancellationToken);

        if (overlaps)
        {
            return (null, "The availability slot overlaps with an existing slot.");
        }

        var slot = new TutorAvailabilitySlot
        {
            Id = Guid.NewGuid(),
            TutorProfileId = profile.Id,
            StartUtc = dto.StartUtc,
            EndUtc = dto.EndUtc
        };

        _db.TutorAvailabilitySlots.Add(slot);
        await _db.SaveChangesAsync(cancellationToken);

        return (new TutorAvailabilitySlotDto(slot.Id, slot.StartUtc, slot.EndUtc), null);
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
