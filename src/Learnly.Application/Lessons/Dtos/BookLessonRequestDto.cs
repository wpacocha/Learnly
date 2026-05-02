using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Lessons.Dtos;

public sealed class BookLessonRequestDto
{
    [Required]
    public Guid TutorAvailabilitySlotId { get; set; }
}
