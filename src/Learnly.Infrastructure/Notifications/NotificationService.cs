using Learnly.Application.Abstractions;
using Learnly.Application.Notifications;
using Learnly.Application.Notifications.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public NotificationService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<NotificationDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return Array.Empty<NotificationDto>();
        }

        await EnsureUpcomingLessonRemindersAsync(userId, cancellationToken);

        return await _db.UserNotifications
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new NotificationDto(x.Id, x.Type, x.Message, x.LinkUrl, x.LessonId, x.CreatedAtUtc, x.ReadAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return;
        }

        var notification = await _db.UserNotifications
            .FirstOrDefaultAsync(x => x.Id == notificationId && x.UserId == userId, cancellationToken);
        if (notification is null || notification.ReadAtUtc is not null)
        {
            return;
        }

        notification.ReadAtUtc = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureUpcomingLessonRemindersAsync(string userId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var remindUntil = now.AddMinutes(30);

        var tutorProfileIds = await _db.TutorProfiles
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        var lessons = await _db.Lessons
            .AsNoTracking()
            .Where(x => x.Status == LessonStatus.Confirmed
                && x.StartUtc > now
                && x.StartUtc <= remindUntil
                && (x.StudentUserId == userId || tutorProfileIds.Contains(x.TutorProfileId)))
            .ToListAsync(cancellationToken);

        foreach (var lesson in lessons)
        {
            var exists = await _db.UserNotifications.AnyAsync(x =>
                x.UserId == userId
                && x.Type == "lesson_reminder"
                && x.LessonId == lesson.Id,
                cancellationToken);
            if (exists)
            {
                continue;
            }

            _db.UserNotifications.Add(new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = "lesson_reminder",
                LessonId = lesson.Id,
                Message = $"Przypomnienie: lekcja zaczyna sie o {lesson.StartUtc.LocalDateTime:g}.",
                LinkUrl = $"/lessons/{lesson.Id}/room",
                CreatedAtUtc = now,
            });
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
