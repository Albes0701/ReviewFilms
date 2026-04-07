# Notifications - List

## 1. Endpoint Summary

- Method: `GET`
- Route: `/api/notifications`
- Controller action: `NotificationsController.GetMyNotifications`
- Auth requirement: `[Authorize]`
- Content-Type: query string
- Business purpose: lay danh sach notification cua current user theo paging

## 2. Source Of Truth

- Controller: `Controllers/NotificationsController.cs`
- Service: `Services/NotificationService.cs` -> `GetUserNotificationsAsync`
- DTO Response: `DTOs/Notifications/NotificationResponse.cs`, `DTOs/Common/PagedResponse.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Query params

- `page` - mac dinh `1`
- `pageSize` - mac dinh `20`, max `100`

### Headers

- Authorization: `Bearer {{access_token}}`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Success`
- Core assertions:
  - `data.items` la array
  - `data.page`, `data.pageSize`, `data.totalCount`, `data.totalPages` ton tai
  - `data.items[*].type` la string enum

### Error

- Khong co token -> `401`
- `page < 1` -> `400`
- `pageSize < 1` -> `400`
- `pageSize > 100` -> `400`

## 5. Test Case Matrix

### Happy Path

- `NOTIFICATIONS_LIST_01` - Verified from code
  - Scenario: list mac dinh
  - Expected: `200`

- `NOTIFICATIONS_LIST_02` - Verified from code
  - Scenario: list voi `page=1&pageSize=5`
  - Expected: `200`

### Validation

- `NOTIFICATIONS_LIST_03` - Verified from code
  - Scenario: `page = 0`
  - Expected: `400`, message xap xi `Page must be greater than zero.`

- `NOTIFICATIONS_LIST_04` - Verified from code
  - Scenario: `pageSize = 0`
  - Expected: `400`

- `NOTIFICATIONS_LIST_05` - Verified from code
  - Scenario: `pageSize = 101`
  - Expected: `400`

### Authentication

- `NOTIFICATIONS_LIST_06` - Verified from code
  - Scenario: thieu token
  - Expected: `401`

### Boundary / Edge

- `NOTIFICATIONS_LIST_07` - Exploratory
  - Scenario: user chua co notification nao
  - Expected: `200`, `items = []`, `totalCount = 0`

## 6. Suggested Postman Setup

- Folder: `Notifications`
- Request name: `List`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Notification page is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.data.items).to.be.an("array");
  pm.expect(json.data.page).to.be.a("number");
});

if (json.data.items.length > 0) {
  pm.environment.set("notification_id", json.data.items[0].id);
}
```

## 7. Coverage Checklist

- [ ] Co case default paging
- [ ] Co case custom paging
- [ ] Co case page/pageSize invalid
- [ ] Co case missing token
- [ ] Co save `notification_id`

