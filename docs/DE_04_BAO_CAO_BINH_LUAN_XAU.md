# Đề 04: Báo Cáo Bình Luận Xấu

## 1. Mục tiêu
Xây dựng chức năng report comment để kiểm tra khả năng thiết kế API có xác thực, validation và xử lý trạng thái nghiệp vụ.

## 2. Yêu cầu đề bài
Hệ thống hiện có bảng `report` nhưng chưa triển khai đầy đủ chức năng. Sinh viên cần xây dựng API để:

- người dùng gửi báo cáo một comment vi phạm
- quản trị viên xem danh sách báo cáo
- quản trị viên cập nhật trạng thái báo cáo

Trạng thái báo cáo gồm:

- `Pending`
- `Reviewed`
- `Rejected`

## 3. Endpoint gợi ý
- `POST /api/reports`
- `GET /api/reports`
- `PATCH /api/reports/{id}`

## 4. Dữ liệu đầu vào

```json
{
  "targetId": "00000000-0000-0000-0000-000000000002",
  "targetType": "Comment",
  "reason": "Nội dung xúc phạm người khác"
}
```

## 5. Kết quả mong đợi
- User gửi report thành công nếu đã đăng nhập.
- Không cho phép gửi report với nội dung rỗng.
- Admin có thể truy xuất danh sách report phân trang.
- Admin có thể đổi trạng thái report.

## 6. Yêu cầu kỹ thuật bắt buộc
- Report phải gắn với người gửi hiện tại.
- Endpoint xem danh sách report phải yêu cầu quyền quản trị.
- Trả lỗi đúng chuẩn khi report không tồn tại.
- Áp dụng DTO request/response đầy đủ.

## 7. Gợi ý chấm điểm
- Thiết kế API hợp lý.
- Validation tốt.
- Phân quyền đúng.
- Xử lý trạng thái chuẩn.