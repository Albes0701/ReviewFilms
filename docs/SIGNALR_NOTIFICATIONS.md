# SignalR Implementation for Notifications Module

## Overview

SignalR has been implemented for the Notifications module to provide real-time notification delivery to connected clients. This enables instant updates when:
- A new notification is created (e.g., comment reply notification)
- A notification is marked as read

## Architecture

### Hub Configuration

**Hub Path:** `/hubs/notifications`

The `NotificationHub` (located in [Hubs/NotificationHub.cs](../Hubs/NotificationHub.cs)) manages WebSocket connections and organizes clients into groups by user ID for targeted messaging.

**User Groups:** 
- Naming convention: `user_{userId}`
- Each connected client automatically subscribes to its user's group upon connection
- Ensures notifications are delivered only to the intended recipient

### Hub Methods

#### Client-to-Server Methods

```csharp
// Explicitly subscribe to notifications (auto-called on connect)
SubscribeToNotifications(Guid userId)

// Unsubscribe from notifications
UnsubscribeFromNotifications(Guid userId)
```

#### Server-to-Client Events

```csharp
// Sent when a new notification is created
ReceiveNotification(NotificationResponse response)

// Sent when a notification is marked as read
NotificationMarkedAsRead(object data) // { notificationId, userId }

// Confirmation after successful subscription
SubscriptionConfirmed(object data) // { message }

// Confirmation after successful unsubscription
UnsubscriptionConfirmed(object data) // { message }
```

## Implementation Details

### 1. Package Registration

**File:** [ReviewFilms.csproj](../ReviewFilms.csproj)
- Added: `Microsoft.AspNetCore.SignalR` v10.0.1

### 2. Service Configuration

**File:** [Program.cs](../Program.cs)
```csharp
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/hubs/notifications");
```

### 3. Service Integration

**File:** [Services/NotificationService.cs](../Services/NotificationService.cs)
- Injected: `IHubContext<NotificationHub> _hubContext`
- On `CreateNotificationAsync()`: 
  - Saves notification to database
  - Broadcasts to user's group via `SendAsync("ReceiveNotification", response)`
- On `MarkAsReadAsync()`:
  - Updates notification status in database
  - Broadcasts event via `SendAsync("NotificationMarkedAsRead", data)`

### 4. Security

- `[Authorize]` attribute on `NotificationHub` ensures only authenticated users can connect
- User ID validation in hub methods prevents cross-user notification access
- Group-based routing ensures notifications only reach the intended recipient

## Client Integration Example

### JavaScript/TypeScript (WebSocket Client)

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/notifications", {
        accessTokenFactory: () => localStorage.getItem("token")
    })
    .withAutomaticReconnect()
    .build();

// Listen for new notifications
connection.on("ReceiveNotification", (notification) => {
    console.log("New notification:", notification);
    // Update UI with new notification
});

// Listen for read status updates
connection.on("NotificationMarkedAsRead", (data) => {
    console.log("Notification marked as read:", data.notificationId);
    // Update notification UI
});

// Connect
connection.start().catch(err => console.error(err));

// Subscribe explicitly (optional, auto-subscribed on connect)
connection.invoke("SubscribeToNotifications", userId);

// Cleanup on disconnect
connection.stop();
```

### .NET Client (for testing)

```csharp
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("https://localhost:5001/hubs/notifications", options =>
    {
        options.AccessTokenProvider = async () => token;
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<NotificationResponse>("ReceiveNotification", notification =>
{
    Console.WriteLine($"Notification: {notification.Title}");
});

connection.On<object>("NotificationMarkedAsRead", data =>
{
    Console.WriteLine("Notification read");
});

await connection.StartAsync();
```

## Event Flow

### 1. New Comment Reply Flow

```
User replies to comment
    ↓
ReviewService.CreateCommentAsync() triggered
    ↓
ReviewService calls CreateReplyNotificationAsync()
    ↓
NotificationService.CreateNotificationAsync() called
    ↓
Notification saved to database
    ↓
SignalR: SendAsync("ReceiveNotification") to user_group
    ↓
Connected clients receive real-time notification
```

### 2. Manual Notification Creation Flow

```
API POST /api/notifications
    ↓
NotificationsController.CreateNotification()
    ↓
NotificationService.CreateNotificationAsync()
    ↓
Notification saved + SignalR event broadcast
    ↓
Recipients notified in real-time
```

### 3. Mark as Read Flow

```
API PATCH /api/notifications/{id}/read
    ↓
NotificationsController.MarkAsRead()
    ↓
NotificationService.MarkAsReadAsync()
    ↓
Updated in database + SignalR event broadcast
    ↓
Connected clients receive read confirmation
```

## Testing

### REST API Endpoints (Existing)

```bash
# Get notifications
GET /api/notifications?page=1&pageSize=20

# Mark notification as read
PATCH /api/notifications/{id}/read

# Create notification manually
POST /api/notifications
```

### WebSocket Testing

**Postman WebSocket:**
1. Connect to: `ws://localhost:5000/hubs/notifications`
2. Include auth header with JWT token
3. Listen for server events (ReceiveNotification, NotificationMarkedAsRead)
4. Send manual test data to verify connectivity

**SignalR Test Console:**
```csharp
var hubConnection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hubs/notifications", opts => {
        opts.AccessTokenProvider = () => Task.FromResult(jwtToken);
    })
    .Build();

await hubConnection.StartAsync();

// Trigger notification creation via API while connected
// Observe "ReceiveNotification" event in console
```

## Configuration Notes

- **CORS:** If frontend is on different domain, configure CORS in Program.cs:
  ```csharp
  builder.Services.AddCors(options =>
  {
      options.AddDefaultPolicy(builder =>
      {
          builder.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
      });
  });
  ```

- **Scaling Considerations:** For production with multiple server instances, implement SignalR backplane (Redis, Azure SignalR Service, etc.)

- **Hub Reconnection:** Client-side: `.WithAutomaticReconnect()` handles network interruptions

## Files Modified/Created

| File | Change |
|------|--------|
| [ReviewFilms.csproj](../ReviewFilms.csproj) | Added Microsoft.AspNetCore.SignalR package |
| [Program.cs](../Program.cs) | Added SignalR registration and hub mapping |
| [Hubs/NotificationHub.cs](../Hubs/NotificationHub.cs) | **NEW** - Hub for managing connections and groups |
| [Services/NotificationService.cs](../Services/NotificationService.cs) | Updated to inject IHubContext and emit SignalR events |
| [Extensions/NotificationModuleExtensions.cs](../Extensions/NotificationModuleExtensions.cs) | No changes (ready for future extensions) |

## Future Enhancements

- [ ] Batch notification broadcasting for performance optimization
- [ ] Connection logging and metrics
- [ ] Typing indicators for real-time reactions
- [ ] Presence detection (online/offline status)
- [ ] Message persistence cache for delayed delivery
- [ ] Rate limiting per connection to prevent abuse
