# Đề 08: Validation Khi Cập Nhật Hồ Sơ Người Dùng

## 1. Mục tiêu
Kiểm tra kỹ năng sử dụng DTO, Data Annotations, validation và format lỗi phản hồi trong ASP.NET Core.

## 2. Yêu cầu đề bài
Mở rộng chức năng cập nhật hồ sơ người dùng hiện tại để bổ sung các ràng buộc dữ liệu sau:

- `DisplayName` không được rỗng.
- `DisplayName` tối đa 100 ký tự.
- `Bio` tối đa 500 ký tự.
- Nếu upload avatar thì chỉ chấp nhận ảnh có phần mở rộng hợp lệ.

## 3. Endpoint áp dụng
- `PUT /api/auth/me`

## 4. Kết quả mong đợi
- Khi dữ liệu hợp lệ, API cập nhật hồ sơ thành công.
- Khi dữ liệu không hợp lệ, API trả về mã 400 với danh sách lỗi rõ ràng.

## 5. Yêu cầu kỹ thuật bắt buộc
- Áp dụng validation trên DTO.
- Không viết validation thủ công tràn lan trong Controller nếu framework đã hỗ trợ.
- Chuẩn hóa response lỗi theo cấu trúc thống nhất của hệ thống.

## 6. Gợi ý chấm điểm
- Validation đúng.
- Thông báo lỗi rõ ràng.
- Không phá vỡ luồng upload hiện có.