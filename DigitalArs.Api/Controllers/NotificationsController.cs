using System.Security.Claims;
using DigitalArs.Application.DTOs.Notifications;
using DigitalArs.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalArs.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out var userId))
            return userId;

        throw new UnauthorizedAccessException("Usuario no autenticado o token inválido.");
    }

    /// <summary>
    /// Obtiene las últimas notificaciones del usuario autenticado.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications([FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, limit, cancellationToken);
        return Ok(notifications);
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas para el badge de la campana.
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(UnreadCountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId, cancellationToken);
        return Ok(new UnreadCountDto { Count = count });
    }

    /// <summary>
    /// Marca una notificación específica como leída.
    /// </summary>
    [HttpPut("{id:int}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var success = await _notificationService.MarkAsReadAsync(userId, id, cancellationToken);
        if (!success)
            return NotFound(new { message = "Notificación no encontrada o no pertenece al usuario." });

        return Ok(new { message = "Notificación marcada como leída." });
    }

    /// <summary>
    /// Marca todas las notificaciones del usuario como leídas.
    /// </summary>
    [HttpPut("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _notificationService.MarkAllAsReadAsync(userId, cancellationToken);
        return Ok(new { message = "Todas las notificaciones fueron marcadas como leídas." });
    }

    /// <summary>
    /// Elimina una notificación específica del usuario autenticado.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(int id, CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var success = await _notificationService.DeleteNotificationAsync(userId, id, cancellationToken);
        if (!success)
            return NotFound(new { message = "Notificación no encontrada o no pertenece al usuario." });

        return Ok(new { message = "Notificación eliminada con éxito." });
    }

    /// <summary>
    /// Elimina todas las notificaciones del usuario autenticado.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllNotifications(CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        await _notificationService.DeleteAllNotificationsAsync(userId, cancellationToken);
        return Ok(new { message = "Todas las notificaciones fueron eliminadas con éxito." });
    }
}

