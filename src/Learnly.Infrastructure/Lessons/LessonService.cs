using Learnly.Application.Abstractions;
using Learnly.Application.Lessons;
using Learnly.Application.Lessons.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Lessons;

public sealed class LessonService : ILessonService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public LessonService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<(LessonDto? lesson, string? error)> BookAsync(BookLessonRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var slot = await _db.TutorAvailabilitySlots
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == dto.TutorAvailabilitySlotId, cancellationToken);
        if (slot is null)
        {
            return (null, "Tutor availability slot not found.");
        }

        if (slot.StartUtc <= DateTimeOffset.UtcNow)
        {
            return (null, "Cannot book a slot in the past.");
        }

        var tutorProfile = await _db.TutorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == slot.TutorProfileId, cancellationToken);
        if (tutorProfile is null)
        {
            return (null, "Tutor profile for slot not found.");
        }

        if (tutorProfile.UserId == userId)
        {
            return (null, "Tutor cannot book own slot.");
        }

        var alreadyBooked = await _db.Lessons
            .AnyAsync(x => x.TutorAvailabilitySlotId == slot.Id && x.Status != LessonStatus.Cancelled, cancellationToken);
        if (alreadyBooked)
        {
            return (null, "This slot is already booked.");
        }

        var lesson = new Lesson
        {
            Id = Guid.NewGuid(),
            StudentUserId = userId,
            TutorProfileId = slot.TutorProfileId,
            TutorAvailabilitySlotId = slot.Id,
            StartUtc = slot.StartUtc,
            EndUtc = slot.EndUtc,
            Status = LessonStatus.Confirmed,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.Lessons.Add(lesson);
        await _db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return (Map(lesson), null);
    }

    public async Task<IReadOnlyList<LessonDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return Array.Empty<LessonDto>();
        }

        var tutorProfileIds = await _db.TutorProfiles
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        return await _db.Lessons
            .AsNoTracking()
            .Where(x => x.StudentUserId == userId || tutorProfileIds.Contains(x.TutorProfileId))
            .OrderBy(x => x.StartUtc)
            .Select(x => Map(x))
            .ToListAsync(cancellationToken);
    }

    public async Task<(LessonDto? lesson, string? error)> ChangeStatusAsync(
        Guid lessonId,
        LessonStatus status,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        var lesson = await _db.Lessons.FirstOrDefaultAsync(x => x.Id == lessonId, cancellationToken);
        if (lesson is null)
        {
            return (null, "Lesson not found.");
        }

        var isTutorOwner = await _db.TutorProfiles
            .AnyAsync(x => x.Id == lesson.TutorProfileId && x.UserId == userId, cancellationToken);
        var isStudent = lesson.StudentUserId == userId;
        if (!isTutorOwner && !isStudent)
        {
            return (null, "You cannot change this lesson.");
        }

        lesson.Status = status;
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(lesson), null);
    }

    private static LessonDto Map(Lesson x) =>
        new(x.Id, x.StudentUserId, x.TutorProfileId, x.TutorAvailabilitySlotId, x.StartUtc, x.EndUtc, x.Status, x.CreatedAtUtc);
}
