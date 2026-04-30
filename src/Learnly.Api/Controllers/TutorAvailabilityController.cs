using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/tutor-availability")]
[Authorize(Policy = "RequireTutor")]
public sealed class TutorAvailabilityController : ControllerBase
{
    private readonly ITutorAvailabilityService _service;

    public TutorAvailabilityController(ITutorAvailabilityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TutorAvailabilitySlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TutorAvailabilitySlotDto>>> GetMine(CancellationToken cancellationToken)
    {
        var data = await _service.GetMineAsync(cancellationToken);
        return Ok(data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TutorAvailabilitySlotDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TutorAvailabilitySlotDto>> Create(
        [FromBody] TutorAvailabilitySlotCreateDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (slot, error) = await _service.CreateMineAsync(dto, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/tutor-availability/{slot!.Id}", slot);
    }

    [HttpDelete("{slotId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid slotId, CancellationToken cancellationToken)
    {
        var error = await _service.DeleteMineAsync(slotId, cancellationToken);
        if (error is not null)
        {
            return NotFound(new { message = error });
        }

        return NoContent();
    }
}
