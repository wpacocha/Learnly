using Learnly.Application.Tutors.Dtos;

namespace Learnly.Application.Tutors;

public interface ITutorProfileService
{
    Task<TutorProfileDto?> GetMineAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns (profile, error). Error is set when a profile already exists.</summary>
    Task<(TutorProfileDto? profile, string? error)> CreateAsync(
        TutorProfileUpsertDto dto,
        CancellationToken cancellationToken = default);

    /// <summary>Returns (profile, error). Error is set when no profile exists.</summary>
    Task<(TutorProfileDto? profile, string? error)> UpdateAsync(
        TutorProfileUpsertDto dto,
        CancellationToken cancellationToken = default);
}
