using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Reviews.Dtos;

public sealed class CreateReviewRequestDto
{
    [Required]
    public Guid LessonId { get; set; }

    [Range(1.0, 5.0)]
    public double Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}
