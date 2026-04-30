using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/tutors/search")]
public sealed class TutorSearchController : ControllerBase
{
    private readonly ITutorSearchService _service;

    public TutorSearchController(ITutorSearchService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TutorSearchResultDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TutorSearchResultDto>>> Search(
        [FromQuery] TutorSearchQueryDto query,
        CancellationToken cancellationToken)
    {
        var results = await _service.SearchAsync(query, cancellationToken);
        return Ok(results);
    }
}
