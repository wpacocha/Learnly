using Learnly.Application.Realtime.Dtos;
using Learnly.Infrastructure.Persistence;
using Learnly.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Learnly.Api.Hubs;

[Authorize]
public sealed class LessonChatHub : Hub
{
    private readonly LearnlyDbContext _db;

    public LessonChatHub(LearnlyDbContext db)
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

    public async Task SendMessage(SendLessonMessageRequestDto dto)
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null || !await HasLessonAccessAsync(dto.LessonId, userId))
        {
            throw new HubException("Access denied.");
        }

        var message = new LessonMessage
        {
            Id = Guid.NewGuid(),
            LessonId = dto.LessonId,
            SenderUserId = userId,
            Message = dto.Message.Trim(),
            SentAtUtc = DateTimeOffset.UtcNow
        };

        _db.LessonMessages.Add(message);
        await _db.SaveChangesAsync();

        await Clients.Group(GroupName(dto.LessonId)).SendAsync("ReceiveMessage", new
        {
            message.Id,
            message.LessonId,
            message.SenderUserId,
            message.Message,
            message.SentAtUtc
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
