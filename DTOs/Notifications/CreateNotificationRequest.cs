using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ReviewFilms.Api.Enums;

namespace ReviewFilms.Api.DTOs.Notifications;

public sealed class CreateNotificationRequest
{
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public NotificationType Type { get; init; }

    [Required]
    [StringLength(255)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Message { get; init; } = string.Empty;

    public JsonElement? Data { get; init; }

    public DateTime? ExpiresAt { get; init; }
}
