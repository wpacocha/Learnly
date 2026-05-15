using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Conversations.Dtos;

public sealed record ConversationDto(
    Guid Id,
    Guid TutorProfileId,
    string TutorName,
    string StudentUserId,
    string StudentEmail,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    string? LastMessage,
    DateTimeOffset? LastMessageAtUtc);

public sealed record ConversationMessageDto(
    Guid Id,
    Guid ConversationId,
    string SenderUserId,
    string SenderDisplayName,
    string Message,
    DateTimeOffset SentAtUtc);

public sealed class StartConversationRequestDto
{
    [Required]
    public Guid TutorProfileId { get; set; }
}

public sealed class SendConversationMessageRequestDto
{
    [Required]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}
