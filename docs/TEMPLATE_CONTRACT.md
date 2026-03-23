# VIBECODE PROJECT CONTRACT
**Dự án:** ReviewFilms API

## 1. SCOPE (PHẠM VI CÔNG VIỆC MVP)
Đây là những tính năng cốt lõi sẽ được thi công trong phiên bản đầu tiên:
- **Core Architecture:** Khởi tạo project ASP.NET Core 10, cấu hình DbContext với PostgreSQL, thiết lập Global Exception Handling và chuẩn hóa Response DTO.
- **Auth & Security:** Đăng ký, Đăng nhập (trả về JWT), phân quyền Role-based cơ bản (Admin/User).
- **Film Module:** Admin có thể CRUD (Tạo, Đọc, Sửa, Xóa) dữ liệu Phim, Thể loại, Người tham gia (thủ công). User có thể lấy danh sách phim, xem chi tiết, và lọc cơ bản. Tích hợp Cloudinary để upload cover/avatar.
- **Review Module:** User có thể chấm điểm (Rating) và viết bình luận (hỗ trợ nested comment). Hệ thống tự tính toán lại điểm trung bình của phim.
- **Notification Module:** Lưu trữ thông báo vào Database khi có tương tác và cung cấp API để lấy danh sách thông báo.

## 2. OUT-OF-SCOPE (NGOÀI PHẠM VI MVP - ĐỂ DÀNH CHO PHASE SAU)
Để đảm bảo ra mắt phiên bản đầu tiên nhanh gọn, các hạng mục sau sẽ **TẠM THỜI CHƯA LÀM**:
- Không làm tính năng Real-time WebSockets (SignalR) cho thông báo và comment (sẽ dùng API pull thông thường ở phase này).
- Không làm tính năng Background Job tự động cào/đồng bộ dữ liệu từ TMDB (Admin nhập tay bằng API trước).
- Không làm Social Login (Google/Facebook SSO) hay tính năng Quên mật khẩu qua Email.

## 3. DEFINITION OF DONE (TIÊU CHUẨN HOÀN THÀNH)
Một hạng mục (JOB) chỉ được coi là hoàn thành khi đáp ứng ĐỦ các tiêu chí sau:
- Code biên dịch thành công, không có warning nghiêm trọng.
- EF Core Migrations chạy thành công và tạo đúng schema dưới PostgreSQL.
- API chạy đúng logic nghiệp vụ, có thể test thành công trên Swagger Interface.
- Dữ liệu trả về luôn được bọc trong DTOs chuẩn định dạng JSON, sẵn sàng để tích hợp mượt mà với các framework frontend hiện đại như React hoặc Next.js.
- Cấu trúc file tuân thủ nghiêm ngặt theo chuẩn đã định nghĩa trong `AGENT.md`.