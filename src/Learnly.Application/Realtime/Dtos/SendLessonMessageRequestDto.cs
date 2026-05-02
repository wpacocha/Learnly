using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Realtime.Dtos;

public sealed class SendLessonMessageRequestDto
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    [MaxLength(4000)]
    public string Message { get; set; } = string.Empty;
}
