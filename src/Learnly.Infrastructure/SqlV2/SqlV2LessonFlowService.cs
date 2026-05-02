using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.SqlV2;

public sealed class SqlV2LessonFlowService : ISqlV2LessonFlowService
{
    private static readonly HashSet<string> RequestStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending", "accepted", "rejected"
    };

    private static readonly HashSet<string> LessonStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "scheduled", "completed", "cancelled"
    };

    private readonly LearnlyDbContext _db;

    public SqlV2LessonFlowService(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task<(SqlLessonRequestDto? request, string? error)> CreateRequestAsync(
        CreateLessonRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.RequestedTimeStart >= request.RequestedTimeEnd)
        {
            return (null, "requested_time_start must be before requested_time_end.");
        }

        var studentExists = await _db.DbUsers.AnyAsync(x => x.UserId == request.StudentId, cancellationToken);
        var offerExists = await _db.DbTutorOffers.AnyAsync(x => x.TutorOffersId == request.TutorOffersId, cancellationToken);
        if (!studentExists || !offerExists)
        {
            return (null, "Student or tutor offer does not exist.");
        }

        var duplicate = await _db.DbLessonRequests.AnyAsync(
            x => x.StudentId == request.StudentId
                 && x.TutorOffersId == request.TutorOffersId
                 && x.RequestedTimeStart == request.RequestedTimeStart,
            cancellationToken);
        if (duplicate)
        {
            return (null, "Duplicate lesson request for the same slot.");
        }

        var entity = new DbLessonRequest
        {
            StudentId = request.StudentId,
            TutorOffersId = request.TutorOffersId,
            RequestedTimeStart = request.RequestedTimeStart,
            RequestedTimeEnd = request.RequestedTimeEnd,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.DbLessonRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(entity), null);
    }

    public async Task<(SqlLessonRequestDto? request, string? error)> ChangeRequestStatusAsync(
        int requestId,
        ChangeLessonRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!RequestStatuses.Contains(request.Status))
        {
            return (null, "Status must be pending/accepted/rejected.");
        }

        var entity = await _db.DbLessonRequests.FirstOrDefaultAsync(x => x.RequestId == requestId, cancellationToken);
        if (entity is null)
        {
            return (null, "Lesson request not found.");
        }

        entity.Status = request.Status;
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(entity), null);
    }

    public async Task<(SqlLessonDto? lesson, string? error)> CreateLessonAsync(
        CreateLessonFromRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var lr = await _db.DbLessonRequests.FirstOrDefaultAsync(x => x.RequestId == request.RequestId, cancellationToken);
        if (lr is null)
        {
            return (null, "Lesson request not found.");
        }

        if (!string.Equals(lr.Status, "accepted", StringComparison.OrdinalIgnoreCase))
        {
            return (null, "Lesson can be created only for accepted requests.");
        }

        var exists = await _db.DbLessons.AnyAsync(x => x.RequestId == request.RequestId, cancellationToken);
        if (exists)
        {
            return (null, "Lesson already exists for this request.");
        }

        var lesson = new DbLesson
        {
            RequestId = request.RequestId,
            Status = "scheduled",
            MeetingUrl = request.MeetingUrl,
            IsPaid = false,
            UpdatedAt = DateTime.UtcNow
        };

        _db.DbLessons.Add(lesson);
        await _db.SaveChangesAsync(cancellationToken);
        return (Map(lesson), null);
    }

    public async Task<IReadOnlyList<SqlLessonRequestDto>> GetRequestsForTutorAsync(int tutorId, CancellationToken cancellationToken = default)
    {
        return await _db.DbLessonRequests
            .AsNoTracking()
            .Join(_db.DbTutorOffers,
                r => r.TutorOffersId,
                o => o.TutorOffersId,
                (r, o) => new { r, o })
            .Where(x => x.o.TutorId == tutorId)
            .OrderByDescending(x => x.r.CreatedAt)
            .Select(x => Map(x.r))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SqlLessonDto>> GetLessonsForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var query = _db.DbLessons
            .AsNoTracking()
            .Join(_db.DbLessonRequests,
                l => l.RequestId,
                r => r.RequestId,
                (l, r) => new { l, r })
            .Join(_db.DbTutorOffers,
                lr => lr.r.TutorOffersId,
                o => o.TutorOffersId,
                (lr, o) => new { lr.l, lr.r, o })
            .Where(x => x.r.StudentId == userId || x.o.TutorId == userId)
            .OrderByDescending(x => x.l.UpdatedAt)
            .Select(x => Map(x.l));

        return await query.ToListAsync(cancellationToken);
    }

    private static SqlLessonRequestDto Map(DbLessonRequest x) =>
        new(x.RequestId, x.StudentId, x.TutorOffersId, x.RequestedTimeStart, x.RequestedTimeEnd, x.Status, x.CreatedAt, x.UpdatedAt);

    private static SqlLessonDto Map(DbLesson x) =>
        new(x.LessonId, x.RequestId, x.Status, x.MeetingUrl, x.IsPaid, x.UpdatedAt);
}
