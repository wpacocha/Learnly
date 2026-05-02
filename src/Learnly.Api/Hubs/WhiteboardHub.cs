using Learnly.Application.Realtime.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Learnly.Api.Hubs;

[Authorize]
public sealed class WhiteboardHub : Hub
{
    private readonly LearnlyDbContext _db;

    public WhiteboardHub(LearnlyDbContext db)
    {
        _db = db;
    }

    public async Task JoinLessonGroup(Guid lessonId)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !await HasLessonAccessAsync(lessonId, userId))
        {
            throw new HubException("Access denied.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lessonId));
    }

    public async Task SendEvent(WhiteboardEventRequestDto dto)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !await HasLessonAccessAsync(dto.LessonId, userId))
        {
            throw new HubException("Access denied.");
        }

        var evt = new WhiteboardEvent
        {
            Id = Guid.NewGuid(),
            LessonId = dto.LessonId,
            SenderUserId = userId,
            EventType = dto.EventType.Trim(),
            PayloadJson = dto.PayloadJson,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        _db.WhiteboardEvents.Add(evt);
        await _db.SaveChangesAsync();

        await Clients.Group(GroupName(dto.LessonId)).SendAsync("ReceiveEvent", new
        {
            evt.Id,
            evt.LessonId,
            evt.SenderUserId,
            evt.EventType,
            evt.PayloadJson,
            evt.CreatedAtUtc
        });
    }

    private async Task<bool> HasLessonAccessAsync(Guid lessonId, string userId)
    {
        return await _db.Lessons.AnyAsync(x => x.Id == lessonId && x.StudentUserId == userId)
            || await _db.Lessons
                .Join(_db.TutorProfiles, l => l.TutorProfileId, t => t.Id, (l, t) => new { l, t })
                .AnyAsync(x => x.l.Id == lessonId && x.t.UserId == userId);
    }

    private static string GroupName(Guid lessonId) => $"lesson:{lessonId}";
}
