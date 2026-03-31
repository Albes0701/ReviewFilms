using System.Text.Json;
using ReviewFilms.Api.DTOs.Common;
using ReviewFilms.Api.DTOs.Notifications;
using ReviewFilms.Api.Entities;
using ReviewFilms.Api.Enums;

var tests = new NotificationResponseTests();
tests.DataJsonIsParsedAsJsonObject();
tests.PagedResponseCalculatesTotalPages();

Console.WriteLine("All notification tests passed.");

sealed class NotificationResponseTests
{
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

        if (!response.Data.HasValue)
        {
            throw new InvalidOperationException("Expected Data to be populated.");
        }

        if (response.Data.Value.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException($"Expected Data to be a JSON object, got {response.Data.Value.ValueKind}.");
        }

        if (response.Data.Value.GetProperty("commentId").GetString() != "33333333-3333-3333-3333-333333333333")
        {
            throw new InvalidOperationException("commentId was not preserved.");
        }
    }

    public void PagedResponseCalculatesTotalPages()
    {
        var response = PagedResponse<int>.Create([1, 2, 3], page: 2, pageSize: 3, totalCount: 10);

        if (response.TotalPages != 4)
        {
            throw new InvalidOperationException($"Expected 4 total pages, got {response.TotalPages}.");
        }

        if (response.Page != 2 || response.PageSize != 3 || response.TotalCount != 10)
        {
            throw new InvalidOperationException("Paging metadata was not preserved.");
        }
    }
}
