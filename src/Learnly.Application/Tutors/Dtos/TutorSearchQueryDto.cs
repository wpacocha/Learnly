using Learnly.Domain.Entities;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorSearchQueryDto
{
    public int? SubjectId { get; set; }
    public int? TeachingLevelId { get; set; }
    public TeachingMode? TeachingMode { get; set; }
    public string? Location { get; set; }
    public DateTimeOffset? AvailableFromUtc { get; set; }
    public DateTimeOffset? AvailableToUtc { get; set; }
}
