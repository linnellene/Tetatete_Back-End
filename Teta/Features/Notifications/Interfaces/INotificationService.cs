using TetaBackend.Domain.Entities;

namespace TetaBackend.Features.Notifications.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationEntity>> GetLatestUserNotifications(Guid userId);
    
    Task ReadNotifications(Guid userId, IEnumerable<Guid> notificationIds);
}