using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorAvailabilitySlotCreateDto
{
    [Required]
    public DateTimeOffset StartUtc { get; set; }

    [Required]
    public DateTimeOffset EndUtc { get; set; }
}
