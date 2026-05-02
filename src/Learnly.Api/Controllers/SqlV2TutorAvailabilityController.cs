using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/v2/tutor-availability")]
public sealed class SqlV2TutorAvailabilityController : ControllerBase
{
    private readonly ISqlV2TutorAvailabilityService _service;

    public SqlV2TutorAvailabilityController(ISqlV2TutorAvailabilityService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SqlTutorAvailabilityDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlTutorAvailabilityDto>> Create([FromBody] CreateTutorAvailabilityRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (slot, error) = await _service.CreateAsync(request, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/v2/tutor-availability/{slot!.TutorAvailabilityId}", slot);
    }

    [HttpGet("{tutorId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<SqlTutorAvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlTutorAvailabilityDto>>> GetForTutor(int tutorId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetForTutorAsync(tutorId, cancellationToken));
    }
}
