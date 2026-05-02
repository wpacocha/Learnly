using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.SqlV2;

public sealed class SqlV2TutorOfferService : ISqlV2TutorOfferService
{
    private static readonly HashSet<string> AllowedLessonTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "online", "in_person", "both"
    };

    private readonly LearnlyDbContext _db;

    public SqlV2TutorOfferService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<(SqlTutorOfferDto? offer, string? error)> CreateAsync(
        CreateTutorOfferRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!AllowedLessonTypes.Contains(request.LessonType))
        {
            return (null, "lesson_type must be: online, in_person, both.");
        }

        if (!string.Equals(request.LessonType, "online", StringComparison.OrdinalIgnoreCase)
            && string.IsNullOrWhiteSpace(request.Localization))
        {
            return (null, "Localization is required for in_person and both.");
        }

        var tutorExists = await _db.DbUsers.AnyAsync(x => x.UserId == request.TutorId, cancellationToken);
        if (!tutorExists)
        {
            return (null, "Tutor user does not exist.");
        }

        var subjectExists = await _db.DbSubjects.AnyAsync(x => x.SubjectId == request.SubjectId, cancellationToken);
        var levelExists = await _db.DbLevels.AnyAsync(x => x.LevelId == request.LevelId, cancellationToken);
        if (!subjectExists || !levelExists)
        {
            return (null, "Subject or level does not exist.");
        }

        var duplicate = await _db.DbTutorOffers.AnyAsync(
            x => x.TutorId == request.TutorId
                 && x.SubjectId == request.SubjectId
                 && x.LevelId == request.LevelId
                 && x.LessonType == request.LessonType,
            cancellationToken);
        if (duplicate)
        {
            return (null, "Offer already exists for this tutor/subject/level/lesson_type.");
        }

        var entity = new DbTutorOffer
        {
            TutorId = request.TutorId,
            SubjectId = request.SubjectId,
            LevelId = request.LevelId,
            LessonType = request.LessonType,
            Localization = request.Localization,
            HourlyRate = request.HourlyRate,
            UpdatedAt = DateTime.UtcNow
        };

        _db.DbTutorOffers.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return (Map(entity), null);
    }

    public async Task<IReadOnlyList<SqlTutorOfferDto>> SearchAsync(
        int? subjectId,
        int? levelId,
        string? lessonType,
        string? localization,
        CancellationToken cancellationToken = default)
    {
        var query = _db.DbTutorOffers.AsNoTracking().AsQueryable();

        if (subjectId.HasValue)
        {
            query = query.Where(x => x.SubjectId == subjectId.Value);
        }

        if (levelId.HasValue)
        {
            query = query.Where(x => x.LevelId == levelId.Value);
        }

        if (!string.IsNullOrWhiteSpace(lessonType))
        {
            query = query.Where(x => x.LessonType == lessonType);
        }

        if (!string.IsNullOrWhiteSpace(localization))
        {
            var loc = localization.Trim().ToLower();
            query = query.Where(x => x.Localization != null && x.Localization.ToLower().Contains(loc));
        }

        return await query
            .OrderBy(x => x.HourlyRate)
            .ThenByDescending(x => x.UpdatedAt)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    private static SqlTutorOfferDto Map(DbTutorOffer x) =>
        new(x.TutorOffersId, x.TutorId, x.SubjectId, x.LevelId, x.LessonType, x.Localization, x.HourlyRate, x.UpdatedAt);
}
