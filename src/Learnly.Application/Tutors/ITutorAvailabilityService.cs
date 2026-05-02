using Learnly.Application.Tutors.Dtos;

namespace Learnly.Application.Tutors;

public interface ITutorAvailabilityService
{
    Task<IReadOnlyList<TutorAvailabilitySlotDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task<(TutorAvailabilitySlotDto? slot, string? error)> CreateMineAsync(
        TutorAvailabilitySlotCreateDto dto,
        CancellationToken cancellationToken = default);

    Task<string?> DeleteMineAsync(Guid slotId, CancellationToken cancellationToken = default);
}
