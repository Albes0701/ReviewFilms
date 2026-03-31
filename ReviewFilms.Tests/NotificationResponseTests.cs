using System.Text.Json;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Notifications;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;
using Xunit;

namespace ReviewFilms.Tests;

public sealed class NotificationResponseTests
{
    [Fact]
    public void DataJsonIsParsedAsJsonObject()
    {
        var notification = new Notification
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Type = NotificationType.CommentReply,
            Title = "Reply received",
            Message = "Someone replied to your comment.",
            DataJson = "{\"commentId\":\"33333333-3333-3333-3333-333333333333\",\"movieId\":\"44444444-4444-4444-4444-444444444444\"}",
            IsRead = false,
            CreatedAt = new DateTime(2026, 3, 30, 0, 0, 0, DateTimeKind.Utc)
        };

        var response = NotificationResponse.FromEntity(notification);

        Assert.True(response.Data.HasValue);
        Assert.Equal(JsonValueKind.Object, response.Data.Value.ValueKind);
        Assert.Equal(
            "33333333-3333-3333-3333-333333333333",
            response.Data.Value.GetProperty("commentId").GetString());
    }

    [Fact]
    public void PagedResponseCalculatesTotalPages()
    {
        var response = PagedResponse<int>.Create([1, 2, 3], page: 2, pageSize: 3, totalCount: 10);

        Assert.Equal(4, response.TotalPages);
        Assert.Equal(2, response.Page);
        Assert.Equal(3, response.PageSize);
        Assert.Equal(10, response.TotalCount);
    }
}
