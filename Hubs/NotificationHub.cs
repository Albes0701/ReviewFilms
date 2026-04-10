using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ReviewFilms.Api.Interfaces;

namespace ReviewFilms.Api.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    private readonly ICurrentUserService _currentUserService;

    public NotificationHub(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = _currentUserService.GetCurrentUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public async Task SubscribeToNotifications(Guid userId)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        if (userId != currentUserId)
        {
            throw new HubException("Unauthorized: Cannot subscribe to other user's notifications.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        await Clients.Caller.SendAsync("SubscriptionConfirmed", new { message = "Successfully subscribed to notifications." });
    }

    public async Task UnsubscribeFromNotifications(Guid userId)
    {
        var currentUserId = _currentUserService.GetCurrentUserId();
        if (userId != currentUserId)
        {
            throw new HubException("Unauthorized: Cannot unsubscribe from other user's notifications.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        await Clients.Caller.SendAsync("UnsubscriptionConfirmed", new { message = "Successfully unsubscribed from notifications." });
    }
}
