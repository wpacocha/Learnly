using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorSubjectsAndLevelsUpsertDto
{
    [Required]
    public List<int> SubjectIds { get; set; } = new();

    [Required]
    public List<int> TeachingLevelIds { get; set; } = new();
}
