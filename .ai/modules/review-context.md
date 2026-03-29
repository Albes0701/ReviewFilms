# NGỮ CẢNH MODULE REVIEW & SOCIAL (JOB-005)

## 1. Trách nhiệm (Role & Scope)
- Bạn phụ trách ĐỘC LẬP module Review (Rating & Nested Comments).
- Entity (`MovieRating`, `Comment` có Self-reference) ĐÃ ĐƯỢC TẠO. KHÔNG sửa file Entity.

## 2. Luật Đăng ký DI & Mocking User Context
- **DI Registration:** Tạo file `Extensions/ReviewModuleExtensions.cs` với hàm `AddReviewModule(...)`.
- **Mocking User Context:** Tính năng comment bắt buộc cần ID của User đang đăng nhập. Bạn PHẢI tạo một interface `ICurrentUserService` trong thư mục `/Interfaces` với hàm `Guid GetCurrentUserId()`. Tạm thời implement một class `MockCurrentUserService` trả về `Guid.NewGuid()` để test. Khi merge, team khác sẽ tiêm implementation thật giải mã từ JWT vào đây.

## 3. Yêu cầu Đầu ra (Deliverables)
- **DTOs (`/DTOs/Reviews`):** `CommentRequest`, `CommentResponse` (có danh sách `ChildComments`), `RatingRequest`.
- **Services (`/Services`):** `IReviewService` (Logic tạo comment đa cấp đảm bảo đúng `ParentId` và `RootId`; Logic thêm Rating và cập nhật `AvgRating` sang bảng Movie).
- **Controllers (`/Controllers`):** `ReviewsController`.
- **Lưu ý Thuật toán:** Thuật toán lấy cây comment (nested comments) phải được thiết kế tối ưu, tránh lỗi truy vấn N+1 của EF Core.