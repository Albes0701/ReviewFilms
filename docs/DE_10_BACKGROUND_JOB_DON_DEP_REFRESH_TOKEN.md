# Đề 10: Xử Lý Tác Vụ Nền Dọn Dẹp Refresh Token Hết Hạn

## 1. Mục tiêu
Kiểm tra khả năng áp dụng Background Service trong ASP.NET Core, đúng với định hướng mở rộng ở phần 2 của báo cáo.

## 2. Yêu cầu đề bài
Sinh viên cần xây dựng một tác vụ nền chạy định kỳ để dọn dẹp các refresh token đã hết hạn hoặc đã bị thu hồi quá lâu.

Yêu cầu nghiệp vụ:

- Cứ mỗi 30 phút, hệ thống tự quét bảng `refresh_token`.
- Xóa các token đã hết hạn.
- Có ghi log số lượng token đã được dọn dẹp.

## 3. Kết quả mong đợi
- Ứng dụng khởi chạy bình thường.
- Background task chạy định kỳ không làm ảnh hưởng luồng API chính.
- Console hoặc logger hiển thị thông tin dọn dẹp.

## 4. Yêu cầu kỹ thuật bắt buộc
- Sử dụng `BackgroundService` hoặc `IHostedService`.
- Tạo scope đúng cách để dùng `DbContext`.
- Không viết job theo kiểu block thread.
- Có xử lý ngoại lệ để job không làm sập ứng dụng.

## 5. Gợi ý chấm điểm
- Job chạy đúng chu kỳ.
- Truy vấn đúng dữ liệu cần dọn.
- Có log và không ảnh hưởng API.