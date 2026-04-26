using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorProfileUpsertDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string TeachingLevel { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0.01", "99999.99")]
    public decimal HourlyRate { get; set; }

    [MaxLength(2000)]
    public string? PhotoUrl { get; set; }
}
