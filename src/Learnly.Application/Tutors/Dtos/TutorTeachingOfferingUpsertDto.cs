using System.ComponentModel.DataAnnotations;
using Learnly.Domain.Entities;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorTeachingOfferingUpsertDto
{
    /// <summary>When set, updates an existing offering for this tutor; when null, a new row is created.</summary>
    public Guid? Id { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public TeachingMode TeachingMode { get; set; }

    [MaxLength(300)]
    public string? Location { get; set; }

    [Range(0.01, 99999.99)]
    public decimal HourlyRate { get; set; }

    [Range(15, 480)]
    public int DurationMinutes { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> TeachingLevelIds { get; set; } = new();
}
