using Microsoft.EntityFrameworkCore;
using TetaBackend.Domain;
using TetaBackend.Domain.Entities;
using TetaBackend.Features.Notifications.Interfaces;

namespace TetaBackend.Features.Notifications;

public class NotificationService : INotificationService
{
    private readonly DataContext _dataContext;
    private const int NotificationsPerRequest = 15;

    public NotificationService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task<IEnumerable<NotificationEntity>> GetLatestUserNotifications(Guid userId)
    {
        return await _dataContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .Take(NotificationsPerRequest)
            .ToListAsync();
    }

    public async Task ReadNotifications(Guid userId, IEnumerable<Guid> notificationIds)
    {
        var notifications = await _dataContext.Notifications
            .Where(n => notificationIds.Contains(n.Id) && n.UserId == userId).ToListAsync();
        
        notifications.ForEach(n => n.IsRead = true);

        await _dataContext.SaveChangesAsync();
    }
}