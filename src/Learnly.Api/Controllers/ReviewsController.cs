using Learnly.Application.Reviews;
using Learnly.Application.Reviews.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public sealed class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize(Policy = "RequireStudent")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequestDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (review, error) = await _reviewService.CreateAsync(dto, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/reviews/{review!.Id}", review);
    }

    [HttpGet("tutor/{tutorProfileId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ReviewDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReviewDto>>> GetForTutor(Guid tutorProfileId, CancellationToken cancellationToken)
    {
        return Ok(await _reviewService.GetForTutorAsync(tutorProfileId, cancellationToken));
    }
}
