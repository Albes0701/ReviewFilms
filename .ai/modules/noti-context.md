# NGỮ CẢNH MODULE NOTIFICATION (JOB-006)

## 1. Trách nhiệm (Role & Scope)
- Bạn phụ trách ĐỘC LẬP module Notification (Core API, chưa làm SignalR).
- Entity `Notification` ĐÃ ĐƯỢC TẠO. KHÔNG sửa file Entity.

## 2. Luật Đăng ký DI & Mocking Chéo
- **DI Registration:** Tạo file `Extensions/NotificationModuleExtensions.cs` với hàm `AddNotificationModule(...)`.
- **Mocking User Context:** Tương tự module Review, tạo `ICurrentUserService` (chứa `GetCurrentUserId()`) và `MockCurrentUserService` để giả lập user lấy thông báo.
- **Cross-module Event Mocking:** Việc tạo thông báo thường xảy ra khi có ai đó comment. Bạn hãy tạo một Service `INotificationService` với hàm `CreateNotificationAsync(...)`. Các module khác sẽ gọi hàm này sau khi merge. Tạm thời, tạo một API POST giả trong Controller để tự test hàm này.

## 3. Yêu cầu Đầu ra (Deliverables)
- **DTOs (`/DTOs/Notifications`):** `NotificationResponse`.
- **Services (`/Services`):** `INotificationService` (Tạo thông báo, Lấy danh sách chưa đọc có phân trang, Đánh dấu đã đọc).
- **Controllers (`/Controllers`):** `NotificationsController` (Cung cấp API cho phép frontend tự pull data).
- **Lưu ý Dữ liệu:** Đảm bảo cột `data_json` lưu dưới dạng cấu trúc JSON hợp lệ trong MySQL và trả về đúng JSON object cho frontend.