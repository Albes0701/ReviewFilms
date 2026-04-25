# Đề 09: Tích Hợp Caching Cho Thể Loại Và Chi Tiết Phim

## 1. Mục tiêu
Áp dụng một hướng mở rộng được nêu trong phần 2 của báo cáo để tối ưu hiệu năng API đọc dữ liệu nhiều lần.

## 2. Yêu cầu đề bài
Sinh viên cần tích hợp cache cho 2 luồng đọc dữ liệu:

- `GET /api/genres`
- `GET /api/movies/{id}`

Khi dữ liệu đã có trong cache, hệ thống không truy vấn lại database.

## 3. Kết quả mong đợi
- Lần gọi đầu lấy dữ liệu từ database và lưu vào cache.
- Các lần gọi tiếp theo lấy từ cache trong thời gian hiệu lực.
- Khi phim được cập nhật, cache chi tiết phim tương ứng phải được làm mới hoặc xóa.

## 4. Yêu cầu kỹ thuật bắt buộc
- Dùng `IMemoryCache`.
- Đặt thời gian sống cache hợp lý, ví dụ 5 phút.
- Không thay đổi response contract hiện có.
- Code cache phải nằm ở tầng Service.

## 5. Gợi ý chấm điểm
- Cache hoạt động đúng.
- Không sinh lỗi dữ liệu cũ không kiểm soát.
- Code tách bạch, dễ bảo trì.