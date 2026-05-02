using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorProfileUpsertDto
{
    [Required]
    [MaxLength(200)]
    public string Headline { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string Bio { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Location { get; set; } = string.Empty;

    [Range(typeof(decimal), "0.01", "99999.99")]
    public decimal HourlyRate { get; set; }

    [MaxLength(2000)]
    public string? PhotoUrl { get; set; }
}
