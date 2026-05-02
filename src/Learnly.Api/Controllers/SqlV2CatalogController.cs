using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/v2/catalog")]
public sealed class SqlV2CatalogController : ControllerBase
{
    private readonly ISqlV2CatalogService _service;

    public SqlV2CatalogController(ISqlV2CatalogService service)
    {
        _service = service;
    }

    [HttpGet("subjects")]
    [ProducesResponseType(typeof(IReadOnlyList<SqlLookupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlLookupDto>>> Subjects(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetSubjectsAsync(cancellationToken));
    }

    [HttpGet("levels")]
    [ProducesResponseType(typeof(IReadOnlyList<SqlLookupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlLookupDto>>> Levels(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetLevelsAsync(cancellationToken));
    }

    [HttpGet("tutors")]
    [ProducesResponseType(typeof(IReadOnlyList<SqlLookupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlLookupDto>>> Tutors(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetTutorsAsync(cancellationToken));
    }
}
