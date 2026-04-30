using Learnly.Application.SqlV2;
using Learnly.Application.SqlV2.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/v2/users")]
public sealed class SqlV2UsersController : ControllerBase
{
    private readonly ISqlV2UserService _service;

    public SqlV2UsersController(ISqlV2UserService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SqlUserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SqlUserDto>>> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetUsersAsync(cancellationToken));
    }

    [HttpPost]
    [ProducesResponseType(typeof(SqlUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlUserDto>> CreateUser([FromBody] CreateSqlUserRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (user, error) = await _service.CreateUserAsync(request, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/v2/users/{user!.UserId}", user);
    }
}
