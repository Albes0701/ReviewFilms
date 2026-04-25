# Đề 07: Bổ Sung Endpoint Quản Trị Có Kiểm Tra Permission

## 1. Mục tiêu
Kiểm tra khả năng áp dụng JWT, claim-based authorization và custom permission trong ASP.NET Core.

## 2. Yêu cầu đề bài
Sinh viên cần bổ sung một endpoint quản trị mới cho module phim, ví dụ:

- `PATCH /api/movies/{id}/publish`

Chức năng:

- chuyển trạng thái phim từ `Draft` sang `Published`
- chỉ tài khoản có permission `movies:publish` mới được thực hiện

## 3. Dữ liệu đầu vào
Request có thể không cần body hoặc chỉ cần body rất đơn giản:

```json
{
  "status": "Published"
}
```

## 4. Kết quả mong đợi
- User không có token: trả 401.
- User có token nhưng thiếu quyền: trả 403.
- User có đúng permission: cập nhật thành công.

## 5. Yêu cầu kỹ thuật bắt buộc
- Sử dụng custom attribute hoặc cơ chế permission sẵn có của dự án.
- Không hard-code kiểm tra role trong Controller.
- Cập nhật dữ liệu thông qua Service.

## 6. Gợi ý chấm điểm
- Phân biệt đúng 401 và 403.
- Permission hoạt động đúng.
- Endpoint đúng chuẩn RESTful.