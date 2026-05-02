namespace Learnly.Domain.Entities;

public sealed class TutorOfferingLevel
{
    public Guid TutorTeachingOfferingId { get; set; }
    public TutorTeachingOffering TutorTeachingOffering { get; set; } = null!;
    public int TeachingLevelId { get; set; }
}
