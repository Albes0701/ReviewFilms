# SignalR Notifications Demo - Quick Start Visual Guide

## 🎯 The Complete Real-time Flow (5 Minutes)

```
╔════════════════════════════════════════════════════════════════════════════╗
║                        DEMO: Comment Reply Notification                    ║
║                                                                            ║
║  Your Postman will show REAL-TIME notification delivery via WebSocket    ║
╚════════════════════════════════════════════════════════════════════════════╝
```

---

## ⚡ Quick Start (Copy-Paste Ready)

### Phase 1: Prepare (2 min)

#### 1️⃣ Import Postman Collection

```
File → Import → Select: ReviewFilms-SignalR-Testing.postman_collection.json
```

#### 2️⃣ Start the API

```bash
cd d:\IT_K22\ReviewFilms
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

#### 3️⃣ Get User Tokens

**Request 1:** POST Login - User A
```
User A email: user.a@example.com
User A pass:  Password@123
→ Save: access_token as {{token_a}}
→ Save: userId as {{user_id_a}}
```

**Request 2:** POST Login - User B
```
User B email: user.b@example.com
User B pass:  Password@123
→ Save: access_token as {{token_b}}
→ Save: userId as {{user_id_b}}
```

#### 4️⃣ Get a Movie ID

**Request:** GET /api/movies?pageSize=10
```
→ Copy: any movie id → save as {{movie_id}}
```

---

### Phase 2: Connect WebSocket (1 min)

#### Left Side - User A Listener

1. Open request: **[WebSocket] User A - Notification Listener**
2. Paste token_a into Authorization header
3. Click **Connect**

```
✅ Connected: WebSocket connected at ws://localhost:5000/hubs/notifications
```

**User A is now listening for notifications** 📡

---

### Phase 3: Trigger Real-time Notification (2 min)

#### Step 1️⃣ User A Creates Root Comment

1. Open request: **[Step 1] User A - Create Root Comment**
2. Replace `{{movie_id}}` with actual movie ID
3. Click **Send**

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "comment-abc123",
    ...
  }
}
```

**Postman auto-saves** `comment_id` ✅

---

#### Step 2️⃣ User B Replies (THIS TRIGGERS THE NOTIFICATION!)

1. Open request: **[Step 2] User B - Reply to Comment**
2. Verify `{{root_comment_id}}` is filled
3. Click **Send**

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "reply-comment-xyz789",
    "parentId": "comment-abc123",
    ...
  }
}
```

---

#### 🔥 Check User A's WebSocket!

**Go back to User A's WebSocket tab and look at the message area:**

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
        "movieId": "...",
        "commentId": "reply-comment-xyz789"
      },
      "isRead": false,
      "createdAt": "2026-04-10T10:05:32Z",
      "readAt": null
    }
  ]
}
```

✨ **THIS IS REAL-TIME MAGIC!** ✨

---

## 📊 Visual Timeline

```
TIME     USER A                           USER B
────────────────────────────────────────────────────────
 T=0     🌐 Open WebSocket
         ↓
         ✅ Connected
         📡 Listening...
         
 T=5sec  Creates comment
         ↓
         📤 POST /api/reviews/comments
         ✅ Response: comment-abc123
         
 T=6sec                                  Creates reply comment
                                          ↓
                                          📤 POST /api/reviews/comments
                                          (parentId: comment-abc123)
         
 T=7sec                                  ✅ Response: reply-comment-xyz789
                                          ↓ Server logic trigger:
                                          → CreateCommentAsync()
                                          → CreateReplyNotificationAsync()
                                          → NotificationService.CreateNotificationAsync()
                                          → _hubContext.SendAsync()
         
 T=8sec  🔔 NOTIFICATION RECEIVED!       
         ↓
         WebSocket message:
         "ReceiveNotification"
         {
           title: "Có người trả lời bình luận",
           message: "...",
           commentId: "reply-comment-xyz789"
         }
         
         📝 User A sees notification
         ✅ Update UI in real-time
```

---

## 🎮 Interactive Demo Checklist

```
SETUP:
☐ API running on http://localhost:5000
☐ Postman collection imported
☐ User A token in {{token_a}}
☐ User B token in {{token_b}}
☐ Movie ID in {{movie_id}}

EXECUTION:
☐ User A connects to WebSocket (observe "Connected")
☐ User A creates root comment
☐ User B creates reply comment
☐ User A receives notification instantly on WebSocket
☐ Notification shows correct data (movieId, commentId, title)

VERIFICATION:
☐ Fetch /api/notifications (User A) - see notification saved in DB
☐ Mark as read - observe NotificationMarkedAsRead event on WebSocket
☐ Open second WebSocket (User B) - verify User B does NOT receive User A's notification
☐ Manual notification - broadcast to all users
```

---

## 🔍 What's Happening Behind the Scenes

```
┌─ SYNCHRONOUS FLOW ─────────────────────────────────┐
│                                                     │
│  1. User B POST /api/reviews/comments              │
│     [parentId: User A's comment]                   │
│                                                     │
│  2. ReviewsController.CreateCommentAsync()         │
│     ↓ Receives request                             │
│                                                     │
│  3. ReviewService.CreateCommentAsync()             │
│     ↓ Saves new comment to DB                      │
│     ↓ Detects parentId != null                     │
│                                                     │
│  4. ReviewService.CreateReplyNotificationAsync()   │
│     ↓ Gets parent author ID (User A)               │
│     ↓ Calls NotificationService                    │
│                                                     │
│  5. NotificationService.CreateNotificationAsync()  │
│     ↓ Saves notification to DB                     │
│     ↓ Injects IHubContext<NotificationHub>         │
│                                                     │
└─ ASYNCHRONOUS (BROADCAST) ────────────────────────┘
                    ↓
      ┌─────────────────────────────┐
      │ _hubContext.Clients         │
      │ .Group("user_userA_id")     │
      │ .SendAsync(                 │
      │   "ReceiveNotification",    │
      │   response                  │
      │ )                           │
      └─────────────────────────────┘
                    ↓
         ✨ WEBSOCKET BROADCAST ✨
                    ↓
      ┌─────────────────────────────┐
      │ User A (connected)          │
      │ ✅ IN GROUP "user_userA"    │
      │ ↓ RECEIVES notification     │
      │                             │
      │ User B (connected)          │
      │ ❌ NOT in that group        │
      │ ↓ DOES NOT receive          │
      │                             │
      │ User C (not connected)      │
      │ ❌ Not listening            │
      │ ↓ Will see in DB later      │
      └─────────────────────────────┘
```

---

## 💬 Expected JSON Messages

### When Notification Created

**Received in WebSocket:**
```json
{
  "type": 1,
  "target": "ReceiveNotification",
  "arguments": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "user-a-id",
      "type": "CommentReply",
      "title": "Có người vừa trả lời bình luận của bạn",
      "message": "Một người dùng vừa phản hồi bình luận của bạn.",
      "dataJson": {
        "movieId": "movie-uuid",
        "commentId": "reply-comment-uuid"
      },
      "isRead": false,
      "readAt": null,
      "expiresAt": null,
      "createdAt": "2026-04-10T10:05:32.123Z"
    }
  ]
}
```

### When Marked as Read

**Received in WebSocket:**
```json
{
  "type": 1,
  "target": "NotificationMarkedAsRead",
  "arguments": [
    {
      "notificationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "userId": "user-a-id"
    }
  ]
}
```

### Subscription Confirmation

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

## 🚀 Advanced - Multi-User Test

```
┌─────────────────────────────────────────────────────────┐
│  TERMINAL 1: Start API                                  │
│  $ cd ReviewFilms && dotnet run                         │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  POSTMAN TAB 1: User A                                  │
│  ├─ WebSocket [User A]                                 │
│  │  ws://localhost:5000/hubs/notifications              │
│  │  Authorization: {{token_a}}                          │
│  │  Status: ✅ Connected                                │
│  │                                                      │
│  ├─ (Waiting for notifications...)                     │
│  └─ (Refresh to see incoming events)                   │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  POSTMAN TAB 2: User B                                  │
│  ├─ POST /api/reviews/comments                         │
│  │  {                                                   │
│  │    "movieId": "{{movie_id}}",                       │
│  │    "parentId": "{{root_comment_id}}",               │
│  │    "content": "Great movie!"                        │
│  │  }                                                   │
│  └─ Click Send → Trigger notification                 │
└─────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────┐
│  RESULT: Tab 1 (User A)                                │
│  Instant WebSocket event received! 🔔                 │
└─────────────────────────────────────────────────────────┘
```

---

## 🐛 Troubleshooting

| Issue | Solution |
|-------|----------|
| **"HTTP 401 Unauthorized"** | Token expired or missing. Get new token from Login requests |
| **"WebSocket upgrade failed"** | Wrong URL or port. Check API is running on 5000 |
| **"No message received"** | Make sure you're triggering from DIFFERENT user (User B must reply) |
| **Connection shows "Connecting..."** | API might be down. Check `dotnet run` output |
| **Get other user's notifications** | This shouldn't happen! Groups prevent it. Check userId is correct |

---

## 📝 Test Scenarios Prepared

1. ✅ **Basic Connection** - WS connect & receive subscription confirmation
2. ✅ **Comment Reply** - Trigger and receive notification
3. ✅ **Mark as Read** - Update status and observe event
4. ✅ **Manual Broadcast** - Admin creates notification for user
5. ✅ **Multi-user Isolation** - Verify users don't receive each other's notifications
6. ✅ **Performance** - Measure latency (<50ms expected)

---

## 🎓 Learning Path

```
1. CONNECT    → Understand WebSocket authentication
              [WebSocket] User A - Notification Listener
              
2. OBSERVE    → See real-time events flowing in
              Create comment, then reply, watch WebSocket
              
3. VALIDATE   → Confirm data arrives correctly
              Check notification JSON structure
              
4. INTEGRATE  → Apply to your frontend
              Use same payload format in your client SDK
              
5. SCALE      → Add more users, test concurrency
              Open multiple WS connections from Postman
```

---

## 📞 Need Help?

Refer to: [docs/SIGNALR_NOTIFICATIONS.md](./SIGNALR_NOTIFICATIONS.md)

Full documentation with client examples, configuration, and production deployment.

---

**Ready? Let's Demo! 🚀**

```
1. Run: dotnet run
2. Import collection in Postman
3. Login (get tokens)
4. Get movie ID
5. Connect User A WebSocket
6. User A: Create comment
7. User B: Reply (trigger!)
8. Watch User A WebSocket receive notification 🎉
```
