# Đề 05: Đếm Thông Báo Chưa Đọc Và Đánh Dấu Tất Cả Đã Đọc

## 1. Mục tiêu
Mở rộng module Notification hiện có để kiểm tra khả năng làm việc với JWT, phân trang, SignalR và cập nhật dữ liệu theo người dùng.

## 2. Yêu cầu đề bài
Hệ thống đã có API lấy danh sách thông báo và đánh dấu một thông báo đã đọc. Sinh viên cần bổ sung thêm:

- `GET /api/notifications/unread-count`: lấy số lượng thông báo chưa đọc.
- `PATCH /api/notifications/read-all`: đánh dấu tất cả thông báo của user hiện tại là đã đọc.

## 3. Kết quả mong đợi
- User đã đăng nhập có thể xem số lượng thông báo chưa đọc của mình.
- Khi gọi `read-all`, toàn bộ thông báo chưa đọc của user đó được cập nhật.
- Nếu hệ thống có tích hợp realtime, có thể phát thêm sự kiện thông báo đã đọc hàng loạt.

## 4. Dữ liệu đầu vào
Không cần body phức tạp. User được xác định thông qua JWT.

## 5. Yêu cầu kỹ thuật bắt buộc
- Không được cập nhật thông báo của user khác.
- Viết truy vấn tối ưu, tránh load không cần thiết.
- Trả về `ApiResponse<T>` thống nhất.
- Sử dụng async/await.

## 6. Gợi ý chấm điểm
- Đúng user context.
- Đếm chính xác unread.
- Đánh dấu hàng loạt thành công.
- Code rõ ràng, đúng module Notification.