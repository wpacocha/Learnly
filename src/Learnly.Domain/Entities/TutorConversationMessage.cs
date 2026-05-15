namespace Learnly.Domain.Entities;

public sealed class TutorConversationMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string SenderUserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset SentAtUtc { get; set; }
}
