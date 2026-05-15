using Learnly.Application.Abstractions;
using Learnly.Application.Conversations;
using Learnly.Application.Conversations.Dtos;
using Learnly.Domain.Entities;
using Learnly.Infrastructure.Identity;
using Learnly.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Infrastructure.Conversations;

public sealed class ConversationService : IConversationService
{
    private readonly LearnlyDbContext _db;
    private readonly ICurrentUser _currentUser;

    public ConversationService(LearnlyDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<ConversationDto>> GetMineAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return Array.Empty<ConversationDto>();
        }

        var tutorProfileIds = await _db.TutorProfiles
            .Where(x => x.UserId == userId)
            .Select(x => x.Id)
            .ToArrayAsync(cancellationToken);

        var conversations = await _db.TutorConversations
            .AsNoTracking()
            .Where(x => x.StudentUserId == userId || tutorProfileIds.Contains(x.TutorProfileId))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return await MapConversationsAsync(conversations, cancellationToken);
    }

    public async Task<(ConversationDto? conversation, string? error)> StartAsync(
        StartConversationRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        var tutor = await _db.TutorProfiles.FirstOrDefaultAsync(x => x.Id == dto.TutorProfileId, cancellationToken);
        if (tutor is null)
        {
            return (null, "Nie znaleziono tutora.");
        }

        if (tutor.UserId == userId)
        {
            return (null, "Nie mozna utworzyc rozmowy z wlasnym profilem tutora.");
        }

        var conversation = await _db.TutorConversations
            .FirstOrDefaultAsync(x => x.TutorProfileId == tutor.Id && x.StudentUserId == userId, cancellationToken);
        if (conversation is null)
        {
            conversation = new TutorConversation
            {
                Id = Guid.NewGuid(),
                TutorProfileId = tutor.Id,
                StudentUserId = userId,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
            };
            _db.TutorConversations.Add(conversation);
            await _db.SaveChangesAsync(cancellationToken);
        }

        var mapped = await MapConversationsAsync(new[] { conversation }, cancellationToken);
        return (mapped[0], null);
    }

    public async Task<(IReadOnlyList<ConversationMessageDto>? messages, string? error)> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        var access = await HasAccessAsync(conversationId, cancellationToken);
        if (!access)
        {
            return (null, "Conversation not found or access denied.");
        }

        var rows = await _db.TutorConversationMessages
            .AsNoTracking()
            .Where(x => x.ConversationId == conversationId)
            .OrderBy(x => x.SentAtUtc)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return (await MapMessagesAsync(rows, cancellationToken), null);
    }

    public async Task<(ConversationMessageDto? message, string? error)> SendMessageAsync(
        Guid conversationId,
        SendConversationMessageRequestDto dto,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return (null, "User is not authenticated.");
        }

        var conversation = await GetAccessibleConversationAsync(conversationId, cancellationToken);
        if (conversation is null)
        {
            return (null, "Conversation not found or access denied.");
        }

        var text = dto.Message.Trim();
        if (string.IsNullOrWhiteSpace(text))
        {
            return (null, "Wiadomosc nie moze byc pusta.");
        }

        var message = new TutorConversationMessage
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderUserId = userId,
            Message = text,
            SentAtUtc = DateTimeOffset.UtcNow,
        };
        conversation.UpdatedAtUtc = message.SentAtUtc;
        _db.TutorConversationMessages.Add(message);
        await UpsertConversationNotificationAsync(conversation, userId, text, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        var mapped = await MapMessagesAsync(new[] { message }, cancellationToken);
        return (mapped[0], null);
    }

    private async Task UpsertConversationNotificationAsync(
        TutorConversation conversation,
        string senderUserId,
        string text,
        CancellationToken cancellationToken)
    {
        var tutorUserId = await _db.TutorProfiles
            .Where(x => x.Id == conversation.TutorProfileId)
            .Select(x => x.UserId)
            .FirstAsync(cancellationToken);
        var recipientUserId = conversation.StudentUserId == senderUserId ? tutorUserId : conversation.StudentUserId;
        var senderName = await ResolveUserDisplayNameAsync(senderUserId, cancellationToken);
        var link = "/conversations";

        var notificationType = $"conversation_message:{senderUserId}";
        var existing = await _db.UserNotifications
            .Where(x => x.UserId == recipientUserId
                && x.Type == notificationType
                && x.LinkUrl == link
                && x.ReadAtUtc == null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var notificationText = $"Nowa wiadomosc od {senderName}: {text}";
        if (notificationText.Length > 1000)
        {
            notificationText = notificationText[..997] + "...";
        }

        if (existing is null)
        {
            _db.UserNotifications.Add(new UserNotification
            {
                Id = Guid.NewGuid(),
                UserId = recipientUserId,
                Type = notificationType,
                Message = notificationText,
                LinkUrl = link,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            });
            return;
        }

        existing.Message = $"Nowe wiadomosci od {senderName}. Ostatnia: {text}";
        if (existing.Message.Length > 1000)
        {
            existing.Message = existing.Message[..997] + "...";
        }
        existing.CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    private async Task<IReadOnlyList<ConversationMessageDto>> MapMessagesAsync(
        IReadOnlyList<TutorConversationMessage> messages,
        CancellationToken cancellationToken)
    {
        var conversationIds = messages.Select(x => x.ConversationId).Distinct().ToArray();
        var conversationParticipants = await _db.TutorConversations
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.Id))
            .Join(_db.TutorProfiles.AsNoTracking(), c => c.TutorProfileId, t => t.Id, (c, t) => new { c, t })
            .GroupJoin(_db.Users.AsNoTracking(), x => x.c.StudentUserId, u => u.Id, (x, users) => new { x.c, x.t, Student = users.FirstOrDefault() })
            .ToDictionaryAsync(x => x.c.Id, cancellationToken);

        return messages
            .Select(x =>
            {
                conversationParticipants.TryGetValue(x.ConversationId, out var participants);
                var senderName = "Uzytkownik";
                if (participants is not null && x.SenderUserId == participants.t.UserId)
                {
                    senderName = $"{participants.t.FirstName} {participants.t.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(senderName))
                    {
                        senderName = "Tutor";
                    }
                }
                else if (participants is not null && x.SenderUserId == participants.c.StudentUserId)
                {
                    senderName = participants.Student?.Email ?? "Uczen";
                }

                return new ConversationMessageDto(
                x.Id,
                x.ConversationId,
                x.SenderUserId,
                senderName,
                x.Message,
                x.SentAtUtc);
            })
            .ToList();
    }

    private async Task<string> ResolveUserDisplayNameAsync(string userId, CancellationToken cancellationToken)
    {
        var tutor = await _db.TutorProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (tutor is not null)
        {
            var tutorName = $"{tutor.FirstName} {tutor.LastName}".Trim();
            if (!string.IsNullOrWhiteSpace(tutorName))
            {
                return tutorName;
            }
        }

        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
        return user?.Email ?? "Uzytkownik";
    }

    private async Task<bool> HasAccessAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        return await GetAccessibleConversationAsync(conversationId, cancellationToken) is not null;
    }

    private async Task<TutorConversation?> GetAccessibleConversationAsync(Guid conversationId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return null;
        }

        return await _db.TutorConversations
            .Join(_db.TutorProfiles, c => c.TutorProfileId, t => t.Id, (c, t) => new { c, t })
            .Where(x => x.c.Id == conversationId && (x.c.StudentUserId == userId || x.t.UserId == userId))
            .Select(x => x.c)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<ConversationDto>> MapConversationsAsync(
        IReadOnlyList<TutorConversation> conversations,
        CancellationToken cancellationToken)
    {
        var tutorIds = conversations.Select(x => x.TutorProfileId).Distinct().ToArray();
        var studentIds = conversations.Select(x => x.StudentUserId).Distinct().ToArray();
        var conversationIds = conversations.Select(x => x.Id).ToArray();

        var tutors = await _db.TutorProfiles
            .AsNoTracking()
            .Where(x => tutorIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var students = await _db.Users
            .AsNoTracking()
            .Where(x => studentIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var lastMessages = await _db.TutorConversationMessages
            .AsNoTracking()
            .Where(x => conversationIds.Contains(x.ConversationId))
            .GroupBy(x => x.ConversationId)
            .Select(g => g.OrderByDescending(x => x.SentAtUtc).First())
            .ToDictionaryAsync(x => x.ConversationId, cancellationToken);

        return conversations.Select(x =>
        {
            tutors.TryGetValue(x.TutorProfileId, out var tutor);
            students.TryGetValue(x.StudentUserId, out ApplicationUser? student);
            lastMessages.TryGetValue(x.Id, out var last);
            var tutorName = tutor is null ? "Tutor" : $"{tutor.FirstName} {tutor.LastName}".Trim();
            return new ConversationDto(
                x.Id,
                x.TutorProfileId,
                string.IsNullOrWhiteSpace(tutorName) ? "Tutor" : tutorName,
                x.StudentUserId,
                student?.Email ?? "Uczen",
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                last?.Message,
                last?.SentAtUtc);
        }).ToList();
    }
}
