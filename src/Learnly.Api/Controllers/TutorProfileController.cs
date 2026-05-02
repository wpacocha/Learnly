using Learnly.Application.Tutors;
using Learnly.Application.Tutors.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/tutor-profile")]
[Authorize(Policy = "RequireTutor")]
public sealed class TutorProfileController : ControllerBase
{
    private const long MaxPhotoBytes = 5 * 1024 * 1024;

    private readonly ITutorProfileService _tutorProfileService;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TutorProfileController> _logger;

    public TutorProfileController(
        ITutorProfileService tutorProfileService,
        IWebHostEnvironment environment,
        ILogger<TutorProfileController> logger)
    {
        _tutorProfileService = tutorProfileService;
        _environment = environment;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(TutorProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TutorProfileDto>> GetMine(CancellationToken cancellationToken)
    {
        var profile = await _tutorProfileService.GetMineAsync(cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpPost("photo")]
    [RequestSizeLimit(MaxPhotoBytes + 64 * 1024)]
    [ProducesResponseType(typeof(UploadPhotoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UploadPhotoResponse>> UploadPhoto(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "Wybierz plik graficzny." });
        }

        if (file.Length > MaxPhotoBytes)
        {
            return BadRequest(new { message = "Maksymalny rozmiar pliku to 5 MB." });
        }

        var ext = file.ContentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => null,
        };

        if (ext is null)
        {
            return BadRequest(new { message = "Dozwolone formaty: JPEG, PNG, WebP." });
        }

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var dir = Path.Combine(webRoot, "uploads", "tutor-photos");
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(dir, fileName);

        await using (var stream = System.IO.File.Create(physicalPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var publicPath = $"/uploads/tutor-photos/{fileName}";
        return Ok(new UploadPhotoResponse(publicPath));
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

        try
        {
            var (profile, error) = await _tutorProfileService.CreateAsync(dto, cancellationToken);
            if (error is not null)
            {
                return Conflict(new { message = error });
            }

            return Created("/api/tutor-profile", profile);
        }
        catch (DbUpdateException ex)
        {
            return DatabaseProblem(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create tutor profile failed.");
            return Problem(
                title: "Błąd podczas tworzenia profilu",
                detail: ex.InnerException?.Message ?? ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
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

        try
        {
            var (profile, error) = await _tutorProfileService.UpdateAsync(dto, cancellationToken);
            if (error is not null)
            {
                return NotFound(new { message = error });
            }

            return Ok(profile);
        }
        catch (DbUpdateException ex)
        {
            return DatabaseProblem(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Update tutor profile failed.");
            return Problem(
                title: "Błąd podczas aktualizacji profilu",
                detail: ex.InnerException?.Message ?? ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private ObjectResult DatabaseProblem(DbUpdateException ex)
    {
        var detail = ex.InnerException?.Message ?? ex.Message;
        return Problem(
            title: "Błąd zapisu w bazie danych",
            detail: detail,
            statusCode: StatusCodes.Status500InternalServerError);
    }

    public sealed record UploadPhotoResponse(string Url);
}
