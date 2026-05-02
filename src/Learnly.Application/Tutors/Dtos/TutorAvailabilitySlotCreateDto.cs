using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorAvailabilitySlotCreateDto
{
    [Required]
    public Guid TutorTeachingOfferingId { get; set; }

    [Required]
    public DateTimeOffset StartUtc { get; set; }
}
