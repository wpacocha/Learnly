using Learnly.Application.Auth;
using Learnly.Application.Lessons;
using Learnly.Application.Lessons.Dtos;
using Learnly.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize]
public sealed class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    [HttpPost("book")]
    [Authorize(Policy = "RequireStudent")]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LessonDto>> Book([FromBody] BookLessonRequestDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (lesson, error) = await _lessonService.BookAsync(dto, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/lessons/{lesson!.Id}", lesson);
    }

    [HttpGet("mine")]
    [ProducesResponseType(typeof(IReadOnlyList<LessonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LessonDto>>> Mine(CancellationToken cancellationToken)
    {
        return Ok(await _lessonService.GetMineAsync(cancellationToken));
    }

    [HttpPatch("{lessonId:guid}/status")]
    [ProducesResponseType(typeof(LessonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LessonDto>> ChangeStatus(
        Guid lessonId,
        [FromQuery] LessonStatus status,
        CancellationToken cancellationToken)
    {
        var (lesson, error) = await _lessonService.ChangeStatusAsync(lessonId, status, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Ok(lesson);
    }
}
