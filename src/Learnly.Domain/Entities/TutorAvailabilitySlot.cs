namespace Learnly.Domain.Entities;

public sealed class TutorAvailabilitySlot
{
    public Guid Id { get; set; }
    public Guid TutorProfileId { get; set; }
    public Guid TutorTeachingOfferingId { get; set; }
    public DateTimeOffset StartUtc { get; set; }
    public DateTimeOffset EndUtc { get; set; }
}
