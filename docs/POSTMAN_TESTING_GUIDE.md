# SignalR Notifications - Postman Testing Guide

## Chuẩn bị trước

### 1. Yêu cầu
- **Postman** (version 10.0+)
- API đang chạy: `http://localhost:5000` hoặc `https://localhost:5001`
- **JWT Token** từ auth flow (xem mục "Lấy Token" ở dưới)
- Postman **WebSocket** feature (có sẵn trong version mới)

### 2. Lấy JWT Token

**Endpoint:** `POST /api/auth/login`

#### Request Body
```json
{
  "email": "user1@example.com",
  "password": "YourPassword123!"
}
```

#### Response (Lưu token này)
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIs...",
    "refreshToken": "...",
    "userId": "550e8400-e29b-41d4-a716-446655440000"
  }
}
```

**Lưu ý:** Sao chép `accessToken` để dùng ở các bước sau

---

## Scenario 1: Kết nối WebSocket Basic

### Bước 1: Mở WebSocket Connection

**Trong Postman:**

1. Tạo request mới → chọn **WebSocket** (chứ không phải HTTP)
2. URL: `ws://localhost:5000/hubs/notifications`
3. **Headers** → Thêm:
   ```
   Authorization: Bearer {accessToken}
   ```
   (Ví dụ: `Bearer eyJhbGciOiJIUzI1NiIs...`)

4. Click **Connect**

#### Expected Response ✅
```
Connected: WebSocket connected at ws://localhost:5000/hubs/notifications
```

### Bước 2: Xác nhận Connected

Sau khi connect, server sẽ tự động thêm bạn vào group `user_{userId}`.

**Trong Console (hoặc Message area)** sẽ thấy:
```json
{
  "type": 1,
  "target": "SubscriptionConfirmed",
  "arguments": [
    {
      "message": "Successfully subscribed to notifications."
    }
  ]
}
```

---

## Scenario 2: Trigger Notification qua API (Comment Reply)

### Tổng quan
```
1. User A bình luận về movie X
   ↓
2. User B reply bình luận của User A
   ↓
3. Notification tự động tạo cho User A
   ↓
4. User A (connected đến WebSocket) nhận instant notification
```

### Bước 1: User A tạo comment gốc

**Endpoint:** `POST /api/reviews/comments`

**Headers:**
```
Authorization: Bearer {userA_token}
Content-Type: application/json
```

**Body:**
```json
{
  "movieId": "7c7d1234-5678-90ab-cdef-1234567890ab",
  "content": "This is a great movie!"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "comment-001",
    "movieId": "7c7d1234-5678-90ab-cdef-1234567890ab",
    "userId": "user-a-id",
    "content": "This is a great movie!",
    "createdAt": "2026-04-10T10:00:00Z"
  }
}
```

**Lưu:** `id` của comment (sẽ dùng ở bước tiếp)

### Bước 2: User B reply comment của User A

**Endpoint:** `POST /api/reviews/comments`

**Headers:**
```
Authorization: Bearer {userB_token}
Content-Type: application/json
```

**Body:**
```json
{
  "movieId": "7c7d1234-5678-90ab-cdef-1234567890ab",
  "parentId": "comment-001",
  "content": "Agreed! Best movie ever!"
}
```

### Bước 3: User A nhận Notification qua WebSocket

**Giả sử User A đang connected đến WebSocket, anh ta sẽ nhận ngay:**

```json
{
  "type": 1,
  "target": "ReceiveNotification",
  "arguments": [
    {
      "id": "notif-uuid",
      "userId": "user-a-id",
      "type": "CommentReply",
      "title": "Có người vừa trả lời bình luận của bạn",
      "message": "Một người dùng vừa phản hồi bình luận của bạn.",
      "dataJson": {
        "movieId": "7c7d1234-5678-90ab-cdef-1234567890ab",
        "commentId": "reply-comment-id"
      },
      "isRead": false,
      "createdAt": "2026-04-10T10:01:23Z",
      "readAt": null
    }
  ]
}
```

✨ **Đó chính là kỳ diệu của realtime!**

---

## Scenario 3: Mark Notification as Read

### Bước 1: API Request - User A đánh dấu notification đã đọc

**Endpoint:** `PATCH /api/notifications/{notificationId}/read`

**Headers:**
```
Authorization: Bearer {userA_token}
```

**Ví dụ Request:**
```
PATCH /api/notifications/notif-uuid/read
```

### Bước 2: User A (WebSocket connected) nhận sự kiện

**Trong WebSocket, User A sẽ thấy:**

```json
{
  "type": 1,
  "target": "NotificationMarkedAsRead",
  "arguments": [
    {
      "notificationId": "notif-uuid",
      "userId": "user-a-id"
    }
  ]
}
```

---

## Scenario 4: Tạo Notification Thủ công

### Goal
Mô phỏng gửi notification từ Admin hoặc thông báo hệ thống

### Bước 1: Call API tạo Notification

**Endpoint:** `POST /api/notifications`

**Headers:**
```
Authorization: Bearer {admin_token}
Content-Type: application/json
```

**Body:**
```json
{
  "type": "CommentReply",
  "title": "🔔 System Notification",
  "message": "Maintenance scheduled for tonight at 10 PM",
  "data": {
    "maintenanceWindow": "22:00 - 23:00 UTC",
    "impact": "Service may be temporarily unavailable"
  },
  "expiresAt": "2026-04-11T00:00:00Z"
}
```

### Bước 2: WebSocket Recipients nhận ngay

**Mỗi user connected sẽ nhận:**
```json
{
  "type": 1,
  "target": "ReceiveNotification",
  "arguments": [
    {
      "id": "system-notif-123",
      "userId": "{target_user_id}",
      "type": "CommentReply",
      "title": "🔔 System Notification",
      "message": "Maintenance scheduled for tonight at 10 PM",
      "dataJson": {...},
      "isRead": false,
      "createdAt": "2026-04-10T10:05:00Z"
    }
  ]
}
```

---

## Scenario 5: Multiple Users Simulation

### Mục đích
Giả lập nhiều users kết nối đồng thời

### Cách làm

**Terminal 1 - User A:**
```
1. Mở Postman Request 1
2. WebSocket: ws://localhost:5000/hubs/notifications
3. Authorization: Bearer {userA_token}
4. Click Connect
5. Chờ nhận notification
```

**Terminal 2 - User B:**
```
1. Mở Postman Request 2 (hoặc tab khác)
2. WebSocket: ws://localhost:5000/hubs/notifications
3. Authorization: Bearer {userB_token}
4. Click Connect
5. Trigger comment reply từ User B
```

**Result:**
- ✅ User B tạo reply comment
- ✅ Notification được tạo cho User A
- ✅ **Chỉ User A nhận** notification (thanks to group isolation `user_userId`)
- ✅ User B không nhận (khác user group)

---

## Complete Test Flow - Step by Step

### Phase 1: Setup

```bash
# 1. Start API
dotnet run --project ReviewFilms.csproj

# 2. Ensure database migrations applied
dotnet ef database update
```

### Phase 2: Postman Setup

**Collection Structure:**
```
📁 ReviewFilms - SignalR Testing
  ├── 🔐 Auth
  │   ├── POST /api/auth/login (User A)
  │   ├── POST /api/auth/login (User B)
  │
  ├── 💬 Reviews - Comments
  │   ├── POST /api/reviews/comments (Create root comment)
  │   ├── POST /api/reviews/comments (Create reply - trigger notif)
  │   ├── GET /api/reviews/movies/{movieId}/comments
  │
  ├── 🔔 Notifications - REST
  │   ├── GET /api/notifications (Fetch notifications)
  │   ├── PATCH /api/notifications/{id}/read
  │   ├── POST /api/notifications (Create manual notif)
  │
  ├── 🌐 WebSocket
  │   ├── WS: User A - Notification Hub
  │   ├── WS: User B - Notification Hub
```

### Phase 3: Execution

```
┌─ USER A TIMELINE ─────────────────────┐
│ 1. Login → Get Token A                │
│ 2. Create root comment (movie X)      │
│ 3. Open WebSocket (User A)            │
│ 4. ✅ Connected - waiting for notifs │
│ 5. [Event received]                   │
│    → NotificationMarkedAsRead          │
│                                       │
└───────────────────────────────────────┘
         ⬆ (triggered by User B)
         │
┌─ USER B TIMELINE ─────────────────────┐
│ 1. Login → Get Token B                │
│ 2. Create reply comment               │
│ 3. → [Server creates notification]    │
│    → [SignalR broadcasts to user_A]   │
│ 4. (Optional) Call mark-as-read API   │
│                                       │
└───────────────────────────────────────┘
```

---

## Common Issues & Troubleshooting

### ❌ "WebSocket upgrade failed"
- **Symptoms:** `HTTP 400` or connection rejected
- **Cause:** Missing/invalid JWT token
- **Fix:**
  ```
  ✓ Verify token is valid (not expired)
  ✓ Check Authorization header format: "Bearer {token}"
  ✓ Check token is from authenticated user
  ```

### ❌ "Unauthorized: Cannot subscribe to other user's notifications"
- **Cause:** Trying to subscribe to different user's group
- **Fix:** Only use your own userId in subscription

### ❌ "Not receiving notifications"
- **Checklist:**
  ```
  ✓ WebSocket connection status is "Connected"
  ✓ Trigger action from DIFFERENT user account
  ✓ Check browser console for errors
  ✓ Verify API call succeeded (HTTP 200)
  ✓ Check firewall/proxy not blocking WebSockets
  ```

### ❌ Connection drops after 30 seconds
- **Cause:** Idle timeout or keep-alive missing
- **Fix:** Client should implement automatic reconnect:
  ```javascript
  .WithAutomaticReconnect([0, 0, 10000])
  ```

---

## Real-time Message Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         ReviewFilms API                         │
└─────────────────────────────────────────────────────────────────┘
                              ⬆︎ ⬇︎
                    (HTTP REST / WebSocket)
        ┌─────────────────────────────────────────────────┐
        │         POST /api/reviews/comments              │
        │      (User B replies to User A's comment)       │
        └─────────────────────────────────────────────────┘
                              │
                              ⬇︎
        ┌─────────────────────────────────────────────────┐
        │   ReviewService.CreateCommentAsync()            │
        │   → Detects this is a reply (parentId != null)  │
        └─────────────────────────────────────────────────┘
                              │
                              ⬇︎
        ┌─────────────────────────────────────────────────┐
        │  CreateReplyNotificationAsync()                 │
        │  → Fetches parent comment author (User A)       │
        │  → Creates notification entity                  │
        └─────────────────────────────────────────────────┘
                              │
                              ⬇︎
        ┌─────────────────────────────────────────────────┐
        │  NotificationService.CreateNotificationAsync()  │
        │  → Save to DB                                   │
        │  → Inject IHubContext                           │
        └─────────────────────────────────────────────────┘
                              │
                              ⬇︎
        ┌─────────────────────────────────────────────────┐
        │  _hubContext.Clients.Group("user_userA_id")     │
        │  .SendAsync("ReceiveNotification", response)    │
        │                                                 │
        │  ✨ BROADCAST TO USER A'S GROUP ✨             │
        └─────────────────────────────────────────────────┘
                              │
                    ┌─────────┴─────────┐
                    │                   │
                    ⬇︎                   ⬇︎
        ┌───────────────────┐  ┌──────────────────┐
        │ User A (connected)│  │ User C (connected)│
        │ ❌ NOT in group   │  │ ❌ NOT in group  │
        │    "user_A"      │  │    "user_A"     │
        └───────────────────┘  └──────────────────┘
                    vs
        ┌─────────────────────────────────────────┐
        │ User A WebSocket Connection             │
        │ ✅ IN GROUP "user_A"                    │
        │ ⬅︎ RECEIVES: {ReceiveNotification}     │
        │ {                                       │
        │   "type": 1,                           │
        │   "target": "ReceiveNotification",     │
        │   "arguments": [{ notification }]      │
        │ }                                       │
        └─────────────────────────────────────────┘
```

---

## Quick Reference - Copy/Paste Examples

### Auth Login (User A)
```rest
POST http://localhost:5000/api/auth/login

{
  "email": "user.a@example.com",
  "password": "Password@123"
}
```

### Get Movies (to find movieId for comment)
```rest
GET http://localhost:5000/api/movies?pageSize=5
```

### Create Root Comment
```rest
POST http://localhost:5000/api/reviews/comments
Authorization: Bearer {tokenA}
Content-Type: application/json

{
  "movieId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "content": "Amazing movie! Loved it."
}
```

### Reply Comment (Trigger Notification)
```rest
POST http://localhost:5000/api/reviews/comments
Authorization: Bearer {tokenB}
Content-Type: application/json

{
  "movieId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "parentId": "comment-uuid-from-previous-response",
  "content": "Totally agree with you!"
}
```

### Get Notifications (User A)
```rest
GET http://localhost:5000/api/notifications?page=1&pageSize=20
Authorization: Bearer {tokenA}
```

### Mark as Read
```rest
PATCH http://localhost:5000/api/notifications/notif-uuid-here/read
Authorization: Bearer {tokenA}
```

### Manual Notification (Admin)
```rest
POST http://localhost:5000/api/notifications
Authorization: Bearer {admin_token}
Content-Type: application/json

{
  "type": "CommentReply",
  "title": "Test Notification",
  "message": "This is a test notification via SignalR",
  "data": {
    "test": true
  }
}
```

---

## WebSocket Connection Example (Postman Settings)

### URL
```
ws://localhost:5000/hubs/notifications
```

### Headers
| Key | Value |
|-----|-------|
| Authorization | Bearer eyJhbGciOiJIUzI1NiIs... |

### Message (Optional - for Subscribe)
```json
{
  "protocol": "json",
  "version": 1
}
```

### Expected Connect Response
```
Connected: WebSocket connected at ws://localhost:5000/hubs/notifications
```

---

## Performance Testing (Advanced)

### Load Test - Multiple Users
```bash
# 1. Open 10 Postman WebSocket connections
# 2. Each from different user (tokenA, tokenB, tokenC, ..., tokenJ)
# 3. Trigger 100 comment replies sequentially
# 4. Measure:
#    - Time to receive notification (latency)
#    - Drop rate (any missed notifications)
#    - Connection stability
```

### Expected Results
```
✅ Latency: < 100ms (usually < 50ms)
✅ Drop Rate: 0%
✅ All 10 users receive correct notification
✅ Each user only receives their own notifications
```

---

## Next Steps

- 📱 [Create Postman Collection](#) (auto-import available)
- 🔗 [Setup Client SDK](../Services/)
- 📊 [Configure Redis for Production Scaling](../docs/)
- 🛡️ [Add Connection Logging & Metrics](../Middlewares/)
