namespace Learnly.Domain.Entities;

public sealed class TutorConversation
{
    public Guid Id { get; set; }
    public Guid TutorProfileId { get; set; }
    public string StudentUserId { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
