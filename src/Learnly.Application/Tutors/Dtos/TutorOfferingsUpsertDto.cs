using System.ComponentModel.DataAnnotations;

namespace Learnly.Application.Tutors.Dtos;

public sealed class TutorOfferingsUpsertDto
{
    [Required]
    [MinLength(1)]
    public List<TutorTeachingOfferingUpsertDto> Offerings { get; set; } = new();
}
