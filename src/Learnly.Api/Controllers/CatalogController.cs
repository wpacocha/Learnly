using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly ITutorCatalogService _service;

    public CatalogController(ITutorCatalogService service)
    {
        _service = service;
    }

    [HttpGet("subjects")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> Subjects(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSubjectsAsync(cancellationToken));
    }

    [HttpGet("teaching-levels")]
    [ProducesResponseType(typeof(IReadOnlyList<LookupItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LookupItemDto>>> TeachingLevels(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetTeachingLevelsAsync(cancellationToken));
    }
}
