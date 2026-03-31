using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Notifications;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class NotificationsController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly INotificationService _notificationService;

    public NotificationsController(
        ICurrentUserService currentUserService,
        INotificationService notificationService)
    {
        _currentUserService = currentUserService;
        _notificationService = notificationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationResponse>>>> GetMyNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        return Ok(ApiResponse<PagedResponse<NotificationResponse>>.Ok(notifications));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<NotificationResponse>>> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var notification = await _notificationService.MarkAsReadAsync(
            userId,
            id,
            cancellationToken);

        return Ok(ApiResponse<NotificationResponse>.Ok(notification));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationResponse>>> CreateNotification(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetCurrentUserId();
        var notification = await _notificationService.CreateNotificationAsync(
            userId,
            request.Type,
            request.Title,
            request.Message,
            request.Data,
            request.ExpiresAt,
            cancellationToken);

        return Ok(ApiResponse<NotificationResponse>.Ok(notification, "Notification created."));
    }
}
