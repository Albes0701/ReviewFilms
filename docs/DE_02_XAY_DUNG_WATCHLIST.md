# Đề 02: Xây Dựng Module Watchlist

## 1. Mục tiêu
Triển khai một module mới cho phép người dùng lưu phim vào danh sách xem sau, bám theo các kiến thức về Controller, Service, EF Core, JWT và thiết kế RESTful API.

## 2. Yêu cầu đề bài
Hệ thống hiện có bảng `watchlist` trong cơ sở dữ liệu nhưng chưa có API hoàn chỉnh. Sinh viên cần xây dựng các endpoint sau:

- `POST /api/watchlist`: thêm phim vào watchlist.
- `GET /api/watchlist`: lấy danh sách watchlist của người dùng hiện tại.
- `PATCH /api/watchlist/{movieId}`: cập nhật trạng thái watchlist.
- `DELETE /api/watchlist/{movieId}`: xóa phim khỏi watchlist.

Trạng thái watchlist gồm:

- `WantToWatch`
- `Watching`
- `Watched`

## 3. Dữ liệu đầu vào
Ví dụ request thêm phim vào watchlist:

```json
{
  "movieId": "00000000-0000-0000-0000-000000000001",
  "status": "WantToWatch"
}
```

## 4. Kết quả mong đợi
- Người dùng đã đăng nhập có thể lưu phim vào danh sách cá nhân.
- Không cho phép cùng một người dùng thêm trùng một phim nhiều lần.
- Có thể cập nhật trạng thái xem.
- Dữ liệu trả về phải đúng định dạng `ApiResponse<T>`.

## 5. Yêu cầu kỹ thuật bắt buộc
- Bắt buộc dùng `[Authorize]`.
- Lấy `userId` từ `ICurrentUserService`, không nhận `userId` từ client.
- Kiểm tra phim có tồn tại trước khi thêm.
- Có validation cho request.
- Viết code theo đúng kiến trúc phân tầng của dự án.

## 6. Gợi ý chấm điểm
- Đúng luồng JWT.
- Không trùng dữ liệu watchlist.
- CRUD hoạt động ổn định.
- Xử lý lỗi hợp lý khi phim không tồn tại hoặc dữ liệu không hợp lệ.