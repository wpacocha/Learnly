using Learnly.Application.Abstractions;
using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Tutors;

public sealed class TutorProfileService : ITutorProfileService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public TutorProfileService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<TutorProfileDto?> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return null;
        }

        var entity = await _db.TutorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        return entity is null ? null : Map(entity);
    }

    public async Task<(TutorProfileDto? profile, string? error)> CreateAsync(
        TutorProfileUpsertDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        if (await _db.TutorProfiles.AnyAsync(p => p.UserId == userId, cancellationToken))
        {
            return (null, "A tutor profile already exists for this account.");
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new TutorProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Bio = dto.Bio.Trim(),
            PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.TutorProfiles.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return (Map(entity), null);
    }

    public async Task<(TutorProfileDto? profile, string? error)> UpdateAsync(
        TutorProfileUpsertDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        var entity = await _db.TutorProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (entity is null)
        {
            return (null, "No tutor profile exists for this account.");
        }

        entity.FirstName = dto.FirstName.Trim();
        entity.LastName = dto.LastName.Trim();
        entity.Bio = dto.Bio.Trim();
        entity.PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl.Trim();
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return (Map(entity), null);
    }

    private static TutorProfileDto Map(TutorProfile p) =>
        new(
            p.Id,
            p.UserId,
            p.FirstName,
            p.LastName,
            p.Bio,
            p.PhotoUrl,
            p.CreatedAtUtc,
            p.UpdatedAtUtc);
}
