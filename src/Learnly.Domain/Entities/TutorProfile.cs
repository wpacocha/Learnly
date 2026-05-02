namespace Learnly.Domain.Entities;

public sealed class TutorProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
