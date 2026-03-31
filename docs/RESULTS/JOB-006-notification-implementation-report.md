# JOB-006: Notification Module - Implementation Report

**Ngày thực hiện:** 2026-03-30  
**Phạm vi:** Core API cho Notification, chưa tích hợp SignalR real-time.

---

## 1. Mục tiêu của JOB-006

JOB-006 triển khai module Notification để ứng dụng có thể:

- lưu thông báo vào cơ sở dữ liệu
- lấy danh sách thông báo của user đang đăng nhập
- đánh dấu thông báo đã đọc
- trả về `DataJson` dưới dạng JSON object thật, không bị escape thành chuỗi

Theo `AGENT.md`, module này phải bám đúng kiến trúc layered của dự án:

- `Controllers` chỉ nhận request và trả response
- `Services` chứa business logic
- `Interfaces` chứa hợp đồng của service
- `DTOs` dùng để chuẩn hóa dữ liệu trả ra client
- không sửa `Entities`
- không sửa `Program.cs`

---

## 2. Tổng quan kiến trúc đã triển khai

Luồng xử lý của Notification hiện tại là:

HTTP request -> `NotificationsController` -> `INotificationService` -> `ApplicationDbContext` -> MySQL

Điểm quan trọng của module này là tách rõ 3 phần:

1. `Controller` dùng `[Authorize]` để bảo vệ API.
2. `CurrentUserService` đọc user id từ claims của JWT.
3. `NotificationService` chịu trách nhiệm tạo, phân trang, và mark-as-read.

Như vậy controller không chứa logic truy vấn DB, và service không phải quan tâm đến chi tiết HTTP.

---

## 3. Danh sách file đã thêm hoặc chỉnh sửa

### File mới

- [DTOs/Notifications/NotificationResponse.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\DTOs\Notifications\NotificationResponse.cs)
- [DTOs/Notifications/CreateNotificationRequest.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\DTOs\Notifications\CreateNotificationRequest.cs)
- [DTOs/Common/PagedResponse.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\DTOs\Common\PagedResponse.cs)
- [Interfaces/ICurrentUserService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Interfaces\ICurrentUserService.cs)
- [Interfaces/INotificationService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Interfaces\INotificationService.cs)
- [Services/NotificationService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Services\NotificationService.cs)
- [Security/MockCurrentUserService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Security\MockCurrentUserService.cs)
- [Extensions/NotificationModuleExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Extensions\NotificationModuleExtensions.cs)
- [Controllers/NotificationsController.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Controllers\NotificationsController.cs)
- [ReviewFilms.Tests/Program.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\ReviewFilms.Tests\Program.cs)
- [ReviewFilms.Tests/ReviewFilms.Tests.csproj](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\ReviewFilms.Tests\ReviewFilms.Tests.csproj)

### File đã chỉnh sửa

- [Extensions/ServiceCollectionExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Extensions\ServiceCollectionExtensions.cs)
- [ReviewFilms.csproj](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\ReviewFilms.csproj)

---

## 4. DTOs

### 4.1 `NotificationResponse`

`NotificationResponse` là DTO trả ra cho client. Nó ánh xạ từ entity `Notification`, nhưng không trả entity trực tiếp.

Nội dung chính:

- `Id`
- `UserId`
- `Type`
- `Title`
- `Message`
- `Data`
- `IsRead`
- `ReadAt`
- `ExpiresAt`
- `CreatedAt`

Điểm quan trọng nhất là `Data`:

- trong entity, `DataJson` là `string?`
- trong response, `Data` là `JsonElement?`

`NotificationResponse.FromEntity(...)` sẽ parse `DataJson` bằng `JsonDocument.Parse(...)` rồi clone `RootElement`.
Nhờ vậy response JSON trả ra là object thật, ví dụ:

```json
{
  "data": {
    "commentId": "..."
  }
}
```

chứ không phải:

```json
{
  "data": "{\"commentId\":\"...\"}"
}
```

Đây là chỗ trực tiếp đáp ứng ràng buộc của JOB-006.

### 4.2 `CreateNotificationRequest`

DTO này phục vụ endpoint tạo notification nội bộ/test.

Nó nhận:

- `Type`
- `Title`
- `Message`
- `Data`
- `ExpiresAt`

`Data` cũng được khai báo là `JsonElement?` để controller nhận JSON object tự nhiên từ request body.

### 4.3 `PagedResponse<T>`

`PagedResponse<T>` chuẩn hóa response phân trang cho endpoint lấy danh sách notification.

Nó giữ:

- `Items`
- `Page`
- `PageSize`
- `TotalCount`
- `TotalPages`

Class này là DTO dùng chung, không phụ thuộc riêng Notification, nên có thể tái sử dụng cho các module khác sau này.

---

## 5. Service Layer

### 5.1 `INotificationService`

Interface này định nghĩa 3 hành vi chính:

1. `CreateNotificationAsync`
2. `GetUserNotificationsAsync`
3. `MarkAsReadAsync`

Thiết kế này bám đúng rule của dự án:

- interface ở `/Interfaces`
- implementation ở `/Services`
- controller chỉ gọi interface

### 5.2 `NotificationService`

`NotificationService` là nơi chứa toàn bộ nghiệp vụ Notification.

#### `CreateNotificationAsync`

Hàm này:

- validate `userId`
- validate `title` và `message`
- validate `Data` phải là JSON object nếu có
- tạo entity `Notification`
- set:
  - `Id = Guid.NewGuid()`
  - `IsRead = false`
  - `ReadAt = null`
  - `CreatedAt = DateTime.UtcNow`
- lưu vào `DbContext`
- trả về `NotificationResponse`

`Data` được lưu bằng `GetRawText()` để giữ nguyên payload JSON gốc trong cột `data_json`.

#### `GetUserNotificationsAsync`

Hàm này:

- kiểm tra `userId`
- validate `page` và `pageSize`
- giới hạn `pageSize` tối đa 100
- query theo `UserId`
- sort `CreatedAt` giảm dần
- tính `TotalCount`
- lấy đúng slice dữ liệu bằng `Skip/Take`
- map từng entity sang `NotificationResponse`
- trả về `PagedResponse<NotificationResponse>`

Điểm đáng chú ý:

- dùng `AsNoTracking()` vì đây là query đọc
- không include navigation `User` vì không cần trả user trong response
- tránh fetch dư dữ liệu

#### `MarkAsReadAsync`

Hàm này:

- kiểm tra `userId`
- kiểm tra `notificationId`
- tìm notification theo `(Id, UserId)` để đảm bảo user chỉ thao tác trên thông báo của chính họ
- nếu không tìm thấy thì ném `KeyNotFoundException`
- nếu chưa đọc thì:
  - set `IsRead = true`
  - set `ReadAt = DateTime.UtcNow`
  - save changes
- trả về notification đã cập nhật

---

## 6. Security / Current User

### `ICurrentUserService`

Interface này chỉ có một nhiệm vụ:

- trả về `Guid GetCurrentUserId()`

### `MockCurrentUserService`

Trong phase hiện tại, dự án chưa có implementation auth đầy đủ, nên service này đóng vai trò lớp đọc user id từ claims.

Nó tìm theo thứ tự:

1. `ClaimTypes.NameIdentifier`
2. `sub`
3. `user_id`

Sau đó parse sang `Guid`.

Nếu không có claim hợp lệ, service ném `UnauthorizedAccessException`.

Ý nghĩa của class này là giữ module Notification có thể chạy độc lập ngay cả khi phần auth chính thức chưa hoàn thiện.

---

## 7. Controller

### `NotificationsController`

Controller này được bảo vệ bằng:

```csharp
[Authorize]
```

Các dependency được inject qua constructor:

- `ICurrentUserService`
- `INotificationService`

#### `GET /api/notifications`

Lấy danh sách notification của user đang đăng nhập.

Query params:

- `page` mặc định `1`
- `pageSize` mặc định `20`

Response:

- `ApiResponse<PagedResponse<NotificationResponse>>`

#### `PATCH /api/notifications/{id}/read`

Đánh dấu notification đã đọc cho user hiện tại.

Response:

- `ApiResponse<NotificationResponse>`

#### `POST /api/notifications`

Endpoint này được giữ để test nội bộ luồng tạo notification.

Trong giai đoạn sau, các module khác như Review/Report sẽ gọi service này trực tiếp khi phát sinh sự kiện.

---

## 8. DI Registration

Do rule của dự án không cho sửa `Program.cs`, module Notification được đăng ký qua extension riêng:

- [Extensions/NotificationModuleExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Extensions\NotificationModuleExtensions.cs)

Extension này đăng ký:

- `AddHttpContextAccessor()`
- `ICurrentUserService -> MockCurrentUserService`
- `INotificationService -> NotificationService`

Sau đó [Extensions/ServiceCollectionExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\review-social\Extensions\ServiceCollectionExtensions.cs) chỉ cần gọi `services.AddNotificationModule();`

Điều này giúp giữ `Program.cs` sạch và phù hợp luật worktree song song.

---

## 9. Xử lý `DataJson`

Đây là phần quan trọng nhất của JOB-006.

### 9.1 Ở tầng entity / database

- `Notification.DataJson` là `string?`
- `ApplicationDbContext` map cột này thành `json` trong MySQL

### 9.2 Ở tầng service

Khi tạo notification:

- nhận `JsonElement?` từ request hoặc từ module gọi sang
- validate chỉ cho phép JSON object
- lưu raw JSON text vào `DataJson`

### 9.3 Ở tầng response

Khi trả về client:

- parse `DataJson`
- expose thành `JsonElement?` trong DTO

Kết quả:

- frontend nhận object JSON thật
- không cần tự `JSON.parse(...)`
- không bị escape string hai lớp

---

## 10. Verification đã chạy

Tôi đã xác nhận:

### Build API

```powershell
dotnet build .\ReviewFilms.csproj
```

Kết quả:

- build thành công
- `0 Warning(s)`
- `0 Error(s)`

### Test harness local

```powershell
dotnet run --project .\ReviewFilms.Tests\ReviewFilms.Tests.csproj
```

Kết quả:

- test pass
- xác nhận `NotificationResponse.Data` là JSON object
- xác nhận `PagedResponse<T>` tính `TotalPages` đúng

### Lý do có `ReviewFilms.Tests`

Project này chỉ là harness kiểm chứng cục bộ để test behavior của module trong môi trường sandbox.

Để tránh `ReviewFilms.Tests` bị compile chung vào Web project do SDK-style globbing, tôi đã loại trừ thư mục này trong `ReviewFilms.csproj`.

---

## 11. Hạn chế hiện tại

Module hiện tại intentionally chưa làm:

- SignalR real-time notification
- background job tự động dọn notification hết hạn
- auth implementation đầy đủ để sinh claim `UserId`
- validation theo business rule phức tạp hơn

Phần này đúng với scope JOB-006: core API trước, realtime sau.

---

## 12. Kết luận

JOB-006 đã hoàn thành phần core của Notification module:

- tạo notification
- lấy danh sách notification có phân trang
- đánh dấu đã đọc
- giữ đúng rule kiến trúc của dự án
- trả `DataJson` dưới dạng object JSON thật

Nếu cần mở rộng ở job sau, module này đã có đủ điểm neo để các module Review/Report gọi trực tiếp `INotificationService` mà không cần sửa kiến trúc hiện tại.

