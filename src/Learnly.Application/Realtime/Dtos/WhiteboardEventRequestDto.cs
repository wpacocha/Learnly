using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Realtime.Dtos;

public sealed class WhiteboardEventRequestDto
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    [MaxLength(100)]
    public string EventType { get; set; } = string.Empty;

    [Required]
    [MaxLength(20000)]
    public string PayloadJson { get; set; } = string.Empty;
}
