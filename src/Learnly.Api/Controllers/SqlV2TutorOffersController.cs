using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/v2/tutor-offers")]
public sealed class SqlV2TutorOffersController : ControllerBase
{
    private readonly ISqlV2TutorOfferService _service;

    public SqlV2TutorOffersController(ISqlV2TutorOfferService service)
    {
        _service = service;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SqlTutorOfferDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlTutorOfferDto>> Create([FromBody] CreateTutorOfferRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (offer, error) = await _service.CreateAsync(request, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/v2/tutor-offers/{offer!.TutorOffersId}", offer);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SqlTutorOfferDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlTutorOfferDto>>> Search(
        [FromQuery] int? subjectId,
        [FromQuery] int? levelId,
        [FromQuery] string? lessonType,
        [FromQuery] string? localization,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.SearchAsync(subjectId, levelId, lessonType, localization, cancellationToken));
    }
}
