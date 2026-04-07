# Notifications - Create

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/notifications`
- Controller action: `NotificationsController.CreateNotification`
- Auth requirement: `[Authorize]`
- Content-Type: `application/json`
- Business purpose: tao notification thu cong de test core notification flow

## 2. Source Of Truth

- Controller: `Controllers/NotificationsController.cs`
- Service: `Services/NotificationService.cs` -> `CreateNotificationAsync`
- DTO Request: `DTOs/Notifications/CreateNotificationRequest.cs`
- DTO Response: `DTOs/Notifications/NotificationResponse.cs`
- Enum: `Enums/NotificationType.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`
- Regression reference: `ReviewFilms.Tests/NotificationResponseTests.cs`

## 3. Request Definition

### Headers

- Authorization: `Bearer {{access_token}}`
- `Content-Type: application/json`

### Body

```json
{
  "type": "System",
  "title": "{{notification_title}}",
  "message": "{{notification_message}}",
  "data": {
    "source": "postman",
    "movieId": "{{movie_id}}"
  }
}
```

### Notes

- `type` duoc serialize dang string
- `data` neu co phai la JSON object, khong phai string/array
- `type` la enum non-nullable; neu bo trong field, binder co the dung gia tri mac dinh `System`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Notification created.`
- Core assertions:
  - `data.id` ton tai
  - `data.type` la string enum
  - `data.title` va `data.message` khop input
  - `data.isRead = false`
  - `data.data` la object neu body co `data`

### Validation error

- Thieu `title` -> `400`
- Thieu `message` -> `400`
- `title` > 255 -> `400`

### Auth / business error

- Khong co token -> `401`
- `data` khong phai object -> `400`
- `type` sai enum -> `400`

## 5. Test Case Matrix

### Happy Path

- `NOTIFICATIONS_CREATE_01` - Verified from code
  - Scenario: tao notification toi thieu
  - Expected: `200`, `isRead = false`

- `NOTIFICATIONS_CREATE_02` - Verified from code
  - Scenario: tao notification co `data` object
  - Expected: `200`, response parse `data` thanh JSON object

- `NOTIFICATIONS_CREATE_03` - Verified from code
  - Scenario: tao notification co `expiresAt`
  - Expected: `200`

- `NOTIFICATIONS_CREATE_04` - Exploratory
  - Scenario: bo trong `type`
  - Expected: co kha nang binder dung mac dinh `System`; collection nen luon gui `type` ro rang

### Validation

- `NOTIFICATIONS_CREATE_05` - Verified from code
  - Scenario: thieu `title`
  - Expected: `400`

- `NOTIFICATIONS_CREATE_06` - Verified from code
  - Scenario: thieu `message`
  - Expected: `400`

- `NOTIFICATIONS_CREATE_07` - Verified from code
  - Scenario: `title` vuot 255 ky tu
  - Expected: `400`

### Authentication

- `NOTIFICATIONS_CREATE_08` - Verified from code
  - Scenario: thieu token
  - Expected: `401`

### Business Rule

- `NOTIFICATIONS_CREATE_09` - Verified from code
  - Scenario: `data` la string hoac array thay vi object
  - Expected: `400`, message xap xi `Notification data must be a JSON object.`

- `NOTIFICATIONS_CREATE_10` - Verified from code
  - Scenario: `type` sai enum
  - Expected: `400`

### Boundary / Edge

- `NOTIFICATIONS_CREATE_11` - Exploratory
  - Scenario: `message` la chuoi chi co space
  - Expected: co the qua validation model nhung fail o service -> `400`

## 6. Suggested Postman Setup

- Folder: `Notifications`
- Request name: `Create`

### Pre-request script

```javascript
const suffix = Date.now().toString();
pm.environment.set("notification_title", `notification_${suffix}`);
pm.environment.set("notification_message", `message_${suffix}`);
```

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Notification create response is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Notification created.");
  pm.expect(json.data.id).to.be.a("string");
  pm.expect(json.data.isRead).to.eql(false);
});

pm.environment.set("notification_id", json.data.id);
```

## 7. Coverage Checklist

- [ ] Co case create toi thieu
- [ ] Co case `data` la object
- [ ] Co case `expiresAt`
- [ ] Co case missing required fields
- [ ] Co case invalid `data`
- [ ] Co case invalid enum
- [ ] Co save `notification_id`
