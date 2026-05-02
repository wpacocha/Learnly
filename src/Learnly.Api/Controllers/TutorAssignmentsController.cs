using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/tutor-assignments")]
[Authorize(Policy = "RequireTutor")]
public sealed class TutorAssignmentsController : ControllerBase
{
    private readonly ITutorAssignmentsService _service;

    public TutorAssignmentsController(ITutorAssignmentsService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TutorTeachingOfferingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TutorTeachingOfferingDto>>> GetMine(CancellationToken cancellationToken)
    {
        var data = await _service.GetMineAsync(cancellationToken);
        return data is null ? NotFound() : Ok(data);
    }

    [HttpPut]
    [ProducesResponseType(typeof(IReadOnlyList<TutorTeachingOfferingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<TutorTeachingOfferingDto>>> Upsert(
        [FromBody] TutorOfferingsUpsertDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (data, error) = await _service.UpsertMineAsync(dto, cancellationToken);
        if (error is not null)
        {
            return NotFound(new { message = error });
        }

        return Ok(data);
    }
}
