namespace Learnly.Domain.Entities;

public sealed class TutorTeachingOffering
{
    public Guid Id { get; set; }
    public Guid TutorProfileId { get; set; }
    public int SubjectId { get; set; }
    public TeachingMode TeachingMode { get; set; }
    public string? Location { get; set; }
    public decimal HourlyRate { get; set; }
    public int DurationMinutes { get; set; }

    public ICollection<TutorOfferingLevel> OfferingLevels { get; set; } = new List<TutorOfferingLevel>();
}
