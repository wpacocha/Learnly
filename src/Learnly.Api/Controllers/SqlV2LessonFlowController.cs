using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/v2/lesson-flow")]
public sealed class SqlV2LessonFlowController : ControllerBase
{
    private readonly ISqlV2LessonFlowService _service;

    public SqlV2LessonFlowController(ISqlV2LessonFlowService service)
    {
        _service = service;
    }

    [HttpPost("requests")]
    [ProducesResponseType(typeof(SqlLessonRequestDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlLessonRequestDto>> CreateRequest([FromBody] CreateLessonRequestRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (result, error) = await _service.CreateRequestAsync(request, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/v2/lesson-flow/requests/{result!.RequestId}", result);
    }

    [HttpPatch("requests/{requestId:int}/status")]
    [ProducesResponseType(typeof(SqlLessonRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlLessonRequestDto>> ChangeRequestStatus(
        int requestId,
        [FromBody] ChangeLessonRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        var (result, error) = await _service.ChangeRequestStatusAsync(requestId, request, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Ok(result);
    }

    [HttpPost("lessons")]
    [ProducesResponseType(typeof(SqlLessonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlLessonDto>> CreateLesson([FromBody] CreateLessonFromRequestRequest request, CancellationToken cancellationToken)
    {
        var (lesson, error) = await _service.CreateLessonAsync(request, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/v2/lesson-flow/lessons/{lesson!.LessonId}", lesson);
    }

    [HttpGet("requests/tutor/{tutorId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<SqlLessonRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlLessonRequestDto>>> RequestsForTutor(int tutorId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetRequestsForTutorAsync(tutorId, cancellationToken));
    }

    [HttpGet("lessons/user/{userId:int}")]
    [ProducesResponseType(typeof(IReadOnlyList<SqlLessonDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlLessonDto>>> LessonsForUser(int userId, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetLessonsForUserAsync(userId, cancellationToken));
    }
}
