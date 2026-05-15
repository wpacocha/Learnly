using Learnly.Application.Notifications;
using Learnly.Application.Notifications.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Learnly.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationsController(INotificationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> Mine(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetMineAsync(cancellationToken));
    }

    [HttpPatch("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
    {
        await _service.MarkAsReadAsync(notificationId, cancellationToken);
        return NoContent();
    }
}
