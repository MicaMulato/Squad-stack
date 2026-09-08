using DigitalArs.Application.DTOs.Notifications;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<NotificationDto> CreateNotificationAsync(
        int userId,
        string title,
        string message,
        string type = "General",
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notification = new Notification
        {
            UserId = userId,
            Title = title.Trim(),
            Message = message.Trim(),
            Type = type,
            ActionUrl = actionUrl,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(notification);
    }

    public async Task<List<NotificationDto>> GetUserNotificationsAsync(
        int userId,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notifications = await repo.Query()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return notifications.Select(MapToDto).ToList();
    }

    public async Task<int> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        return await repo.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notifications = await repo.FindAsync(n => n.Id == notificationId && n.UserId == userId);
        var notification = notifications.FirstOrDefault();

        if (notification == null) return false;

        notification.IsRead = true;
        repo.Update(notification);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var unread = await repo.Query()
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (!unread.Any()) return true;

        foreach (var item in unread)
        {
            item.IsRead = true;
            repo.Update(item);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int userId, int notificationId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var notifications = await repo.FindAsync(n => n.Id == notificationId && n.UserId == userId);
        var notification = notifications.FirstOrDefault();

        if (notification == null) return false;

        repo.Delete(notification);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAllNotificationsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var repo = _unitOfWork.Repository<Notification>();
        var userNotifications = await repo.Query()
            .Where(n => n.UserId == userId)
            .ToListAsync(cancellationToken);

        if (!userNotifications.Any()) return true;

        foreach (var item in userNotifications)
        {
            repo.Delete(item);
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private static NotificationDto MapToDto(Notification n) => new()

    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt,
        ActionUrl = n.ActionUrl
    };
}
