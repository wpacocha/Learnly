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

    public async Task<TutorSubjectsAndLevelsDto?> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return null;
        }

        var subjectIds = await _db.TutorSubjects
            .Where(x => x.TutorProfileId == profile.Id)
            .Select(x => x.SubjectId)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        var levelIds = await _db.TutorTeachingLevels
            .Where(x => x.TutorProfileId == profile.Id)
            .Select(x => x.TeachingLevelId)
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

        return new TutorSubjectsAndLevelsDto(subjectIds, levelIds);
    }

    public async Task<(TutorSubjectsAndLevelsDto? data, string? error)> UpsertMineAsync(
        TutorSubjectsAndLevelsUpsertDto dto,
        CancellationToken cancellationToken = default)
    {
        var profile = await GetMyProfileAsync(cancellationToken);
        if (profile is null)
        {
            return (null, "Create tutor profile first.");
        }

        var subjectIds = dto.SubjectIds.Distinct().OrderBy(x => x).ToArray();
        var levelIds = dto.TeachingLevelIds.Distinct().OrderBy(x => x).ToArray();

        var validSubjects = await _db.Subjects.Where(s => subjectIds.Contains(s.Id)).Select(s => s.Id).ToArrayAsync(cancellationToken);
        if (validSubjects.Length != subjectIds.Length)
        {
            return (null, "One or more subject IDs are invalid.");
        }

        var validLevels = await _db.TeachingLevels.Where(l => levelIds.Contains(l.Id)).Select(l => l.Id).ToArrayAsync(cancellationToken);
        if (validLevels.Length != levelIds.Length)
        {
            return (null, "One or more teaching level IDs are invalid.");
        }

        var oldSubjects = _db.TutorSubjects.Where(x => x.TutorProfileId == profile.Id);
        var oldLevels = _db.TutorTeachingLevels.Where(x => x.TutorProfileId == profile.Id);
        _db.TutorSubjects.RemoveRange(oldSubjects);
        _db.TutorTeachingLevels.RemoveRange(oldLevels);

        _db.TutorSubjects.AddRange(subjectIds.Select(id => new TutorSubject
        {
            TutorProfileId = profile.Id,
            SubjectId = id
        }));

        _db.TutorTeachingLevels.AddRange(levelIds.Select(id => new TutorTeachingLevel
        {
            TutorProfileId = profile.Id,
            TeachingLevelId = id
        }));

        await _db.SaveChangesAsync(cancellationToken);

        return (new TutorSubjectsAndLevelsDto(subjectIds, levelIds), null);
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
