using Learnly.Application.Conversations;
using Learnly.Application.Conversations.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
public sealed class ConversationsController : ControllerBase
{
    private readonly IConversationService _service;

    public ConversationsController(IConversationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ConversationDto>>> Mine(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetMineAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ConversationDto>> Start(
        [FromBody] StartConversationRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (conversation, error) = await _service.StartAsync(dto, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Ok(conversation);
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<ConversationMessageDto>>> Messages(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var (messages, error) = await _service.GetMessagesAsync(conversationId, cancellationToken);
        if (error is not null)
        {
            return NotFound(new { message = error });
        }

        return Ok(messages);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<ConversationMessageDto>> Send(
        Guid conversationId,
        [FromBody] SendConversationMessageRequestDto dto,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var (message, error) = await _service.SendMessageAsync(conversationId, dto, cancellationToken);
        if (error is not null)
        {
            return BadRequest(new { message = error });
        }

        return Created($"/api/conversations/{conversationId}/messages/{message!.Id}", message);
    }
}
