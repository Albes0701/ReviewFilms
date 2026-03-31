using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReviewFilms.Api.Data;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Notifications;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Services;

public sealed class NotificationService : INotificationService
{
    private const int MaxPageSize = 100;

    private readonly ApplicationDbContext _dbContext;

    public NotificationService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<NotificationResponse> CreateNotificationAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        JsonElement? data = null,
        DateTime? expiresAt = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateText(title, nameof(title));
        ValidateText(message, nameof(message));
        ValidateNotificationData(data);

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Title = title.Trim(),
            Message = message.Trim(),
            DataJson = data.HasValue ? data.Value.GetRawText() : null,
            IsRead = false,
            ReadAt = null,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return NotificationResponse.FromEntity(notification);
    }

    public async Task<PagedResponse<NotificationResponse>> GetUserNotificationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidatePaging(page, pageSize);

        var query = _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = notifications
            .Select(NotificationResponse.FromEntity)
            .ToArray();

        return PagedResponse<NotificationResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<NotificationResponse> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        if (notificationId == Guid.Empty)
        {
            throw new ArgumentException("Notification id is required.", nameof(notificationId));
        }

        var notification = await _dbContext.Notifications
            .FirstOrDefaultAsync(
                item => item.Id == notificationId && item.UserId == userId,
                cancellationToken);

        if (notification is null)
        {
            throw new KeyNotFoundException("Notification not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return NotificationResponse.FromEntity(notification);
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }
    }

    private static void ValidateText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"{parameterName} is required.", parameterName);
        }
    }

    private static void ValidateNotificationData(JsonElement? data)
    {
        if (!data.HasValue)
        {
            return;
        }

        if (data.Value.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Notification data must be a JSON object.", nameof(data));
        }
    }

    private static void ValidatePaging(int page, int pageSize)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be greater than zero.", nameof(page));
        }

        if (pageSize < 1)
        {
            throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));
        }

        if (pageSize > MaxPageSize)
        {
            throw new ArgumentException($"Page size cannot exceed {MaxPageSize}.", nameof(pageSize));
        }
    }
}
