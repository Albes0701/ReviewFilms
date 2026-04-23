# Notifications - Mark As Read

## 1. Endpoint Summary

- Method: `PATCH`
- Route: `/api/notifications/{id}/read`
- Controller action: `NotificationsController.MarkAsRead`
- Auth requirement: `[Authorize]`
- Content-Type: path param
- Business purpose: danh dau notification cua current user da doc

## 2. Source Of Truth

- Controller: `Controllers/NotificationsController.cs`
- Service: `Services/NotificationService.cs` -> `MarkAsReadAsync`
- DTO Response: `DTOs/Notifications/NotificationResponse.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Path params

- `id` - GUID cua notification

### Headers

- Authorization: `Bearer {{access_token}}`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Success`
- Core assertions:
  - `data.id = {{notification_id}}`
  - `data.isRead = true`
  - `data.readAt` khac `null`

### Error

- `id` sai GUID -> `400`
- Khong co token -> `401`
- Notification khong ton tai hoac khong thuoc user hien tai -> `404`

## 5. Test Case Matrix

### Happy Path

- `NOTIFICATIONS_READ_01` - Verified from code
  - Scenario: mark notification unread thanh read
  - Expected: `200`, `isRead = true`

- `NOTIFICATIONS_READ_02` - Verified from code
  - Scenario: mark lai notification da read
  - Expected: `200`, van thanh cong, khong throw loi

### Validation

- `NOTIFICATIONS_READ_03` - Verified from code
  - Scenario: `id` sai GUID
  - Expected: `400`

### Authentication

- `NOTIFICATIONS_READ_04` - Verified from code
  - Scenario: thieu token
  - Expected: `401`

### Authorization / Ownership

- `NOTIFICATIONS_READ_05` - Verified from code
  - Scenario: notification thuoc user khac
  - Expected: `404`

### Business Rule

- `NOTIFICATIONS_READ_06` - Verified from code
  - Scenario: notification khong ton tai
  - Expected: `404`, message `Notification not found.`

## 6. Suggested Postman Setup

- Folder: `Notifications`
- Request name: `Mark As Read`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Notification is marked as read", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.data.id).to.eql(pm.environment.get("notification_id"));
  pm.expect(json.data.isRead).to.eql(true);
  pm.expect(json.data.readAt).to.not.eql(null);
});
```

## 7. Coverage Checklist

- [ ] Co case mark unread -> read
- [ ] Co case mark read lan 2
- [ ] Co case invalid guid
- [ ] Co case missing token
- [ ] Co case ownership/not found

