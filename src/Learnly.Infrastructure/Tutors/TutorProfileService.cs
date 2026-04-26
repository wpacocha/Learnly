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
            Subject = dto.Subject.Trim(),
            TeachingLevel = dto.TeachingLevel.Trim(),
            Location = dto.Location.Trim(),
            Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
            HourlyRate = dto.HourlyRate,
            PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl.Trim(),
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        _db.TutorProfiles.Add(entity);
        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return (null, "Nie udało się zapisać profilu. Upewnij się, że konto tutora jest poprawne i spróbuj ponownie.");
        }

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
        entity.Subject = dto.Subject.Trim();
        entity.TeachingLevel = dto.TeachingLevel.Trim();
        entity.Location = dto.Location.Trim();
        entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
        entity.HourlyRate = dto.HourlyRate;
        entity.PhotoUrl = string.IsNullOrWhiteSpace(dto.PhotoUrl) ? null : dto.PhotoUrl.Trim();
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return (null, "Nie udało się zaktualizować profilu.");
        }

        return (Map(entity), null);
    }

    private static TutorProfileDto Map(TutorProfile p) =>
        new(
            p.Id,
            p.UserId,
            p.FirstName,
            p.LastName,
            p.Subject,
            p.TeachingLevel,
            p.Location,
            p.Description,
            p.HourlyRate,
            p.PhotoUrl,
            p.CreatedAtUtc,
            p.UpdatedAtUtc);
}
