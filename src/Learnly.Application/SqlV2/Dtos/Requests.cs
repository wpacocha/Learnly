using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.SqlV2.Dtos;

public sealed class CreateTutorOfferRequest
{
    [Required]
    public int TutorId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int LevelId { get; set; }

    [Required]
    [MaxLength(10)]
    public string LessonType { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Localization { get; set; }

    [Range(0.01, 99999.99)]
    public decimal HourlyRate { get; set; }
}

public sealed class CreateTutorAvailabilityRequest
{
    [Required]
    public int TutorId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [MaxLength(255)]
    public string? Localization { get; set; }
}

public sealed class CreateLessonRequestRequest
{
    [Required]
    public int StudentId { get; set; }

    [Required]
    public int TutorOffersId { get; set; }

    [Required]
    public DateTime RequestedTimeStart { get; set; }

    [Required]
    public DateTime RequestedTimeEnd { get; set; }
}

public sealed class ChangeLessonRequestStatusRequest
{
    [Required]
    [MaxLength(10)]
    public string Status { get; set; } = string.Empty;
}

public sealed class CreateLessonFromRequestRequest
{
    [Required]
    public int RequestId { get; set; }

    [MaxLength(500)]
    public string? MeetingUrl { get; set; }
}
