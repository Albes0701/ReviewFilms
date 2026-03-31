using System.Text.Json;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Notifications;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.Interfaces;

public interface INotificationService
{
    Task<NotificationResponse> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        JsonElement? data = null,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<NotificationResponse>> GetUserNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<NotificationResponse> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);
}
