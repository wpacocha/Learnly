using Learnly.Application.Notifications.Dtos;

namespace Learnly.Application.Notifications;

public interface INotificationService
{
    Task<IReadOnlyList<NotificationDto>> GetMineAsync(CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
