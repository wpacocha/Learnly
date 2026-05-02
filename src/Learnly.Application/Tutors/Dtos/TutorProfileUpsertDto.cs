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
    [MaxLength(4000)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? PhotoUrl { get; set; }
}
