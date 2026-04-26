using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/tutor-profile")]
[Authorize(Policy = "RequireTutor")]
public sealed class TutorProfileController : ControllerBase
{
    private readonly ITutorProfileService _tutorProfileService;

    public TutorProfileController(ITutorProfileService tutorProfileService)
    {
        _tutorProfileService = tutorProfileService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TutorProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorProfileDto>> GetMine(CancellationToken cancellationToken)
    {
        var profile = await _tutorProfileService.GetMineAsync(cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TutorProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TutorProfileDto>> Create(
        [FromBody] TutorProfileUpsertDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (profile, error) = await _tutorProfileService.CreateAsync(dto, cancellationToken);
        if (error is not null)
        {
            if (error.Contains("already exists", StringComparison.OrdinalIgnoreCase))
            {
                return Conflict(new { message = error });
            }

            return BadRequest(new { message = error });
        }

        return Created("/api/tutor-profile", profile);
    }

    [HttpPut]
    [ProducesResponseType(typeof(TutorProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorProfileDto>> Update(
        [FromBody] TutorProfileUpsertDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (profile, error) = await _tutorProfileService.UpdateAsync(dto, cancellationToken);
        if (error is not null)
        {
            if (error.Contains("No tutor profile exists", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { message = error });
            }

            return BadRequest(new { message = error });
        }

        return Ok(profile);
    }
}
