# Đề 06: Chống Spam Bình Luận

## 1. Mục tiêu
Thêm một ràng buộc nghiệp vụ nhỏ nhưng thực tế vào module comment, tương tự tinh thần bài mẫu yêu cầu thêm cooldown cho hành động trong code hiện có.

## 2. Yêu cầu đề bài
Khi người dùng gửi bình luận liên tiếp trong thời gian quá ngắn, hệ thống phải chặn thao tác để tránh spam.

Luật nghiệp vụ:

- Một user không được tạo 2 comment trong vòng 10 giây trên cùng một phim.
- Nếu vi phạm, hệ thống trả về lỗi nghiệp vụ rõ ràng.

## 3. Endpoint áp dụng
- `POST /api/reviews/comments`

## 4. Dữ liệu đầu vào
Giữ nguyên cấu trúc request comment hiện có.

## 5. Kết quả mong đợi
- Nếu user comment hợp lệ, hệ thống vẫn tạo comment bình thường.
- Nếu user comment quá nhanh, hệ thống từ chối và trả thông báo phù hợp.

## 6. Yêu cầu kỹ thuật bắt buộc
- Kiểm tra thời gian comment gần nhất của user trên phim đang thao tác.
- Không thay đổi schema database nếu không thật sự cần.
- Không viết logic ở Controller.
- Xử lý bằng Service và để Global Exception Middleware format lỗi.

## 7. Gợi ý chấm điểm
- Đúng logic cooldown.
- Không làm hỏng luồng comment cũ.
- Thông báo lỗi rõ ràng.
- Code gọn và đúng nơi.