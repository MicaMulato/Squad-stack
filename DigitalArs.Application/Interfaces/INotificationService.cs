using DigitalArs.Application.DTOs.Notifications;

namespace DigitalArs.Application.Interfaces;

public interface INotificationService
{
    Task<NotificationDto> CreateNotificationAsync(int userId, string title, string message, string type = "General", string? actionUrl = null, CancellationToken cancellationToken = default);
    Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, int limit = 20, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(int userId, int notificationId, CancellationToken cancellationToken = default);
    Task<bool> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteNotificationAsync(int userId, int notificationId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAllNotificationsAsync(int userId, CancellationToken cancellationToken = default);
}

