using System.Text.Json;
using System.Text.Json.Serialization;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Notifications;

public sealed class NotificationResponse
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public JsonElement? Data { get; init; }

    public bool IsRead { get; init; }

    public DateTime? ReadAt { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public DateTime CreatedAt { get; init; }

    public static NotificationResponse FromEntity(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);

        return new NotificationResponse
        {
            Id = notification.Id,
            UserId = notification.UserId,
            Type = notification.Type,
            Title = notification.Title,
            Message = notification.Message,
            Data = ParseData(notification.DataJson),
            IsRead = notification.IsRead,
            ReadAt = notification.ReadAt,
            ExpiresAt = notification.ExpiresAt,
            CreatedAt = notification.CreatedAt
        };
    }

    private static JsonElement? ParseData(string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        using var document = JsonDocument.Parse(dataJson);
        return document.RootElement.Clone();
    }
}
