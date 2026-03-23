# VIBECODE PROJECT INTAKE 
**Tên dự án:** ReviewFilms API (Mạng xã hội đánh giá phim)
**Vai trò:** Backend API phục vụ các nền tảng Web/Mobile.

## 1. MỤC TIÊU CỐT LÕI
Xây dựng một hệ thống backend nguyên khối (Monolithic) cung cấp API cho nền tảng mạng xã hội về phim ảnh, nơi người dùng có thể tra cứu thông tin phim, đánh giá, bình luận và tương tác với nhau.

## 2. CÁC MODULE CHÍNH (SCOPE)
- **Module Auth & Security:** Quản lý User, Role, Permission (RBAC), JWT Token, Refresh Token.
- **Module Film:** Quản lý danh mục Phim, Thể loại (Genres), Đạo diễn/Diễn viên (Persons & Credits), Watchlist. Tích hợp lấy dữ liệu từ TMDB API.
- **Module Review & Social:** Đánh giá điểm (Rating), Bình luận đa cấp (Nested Comments), Upvote/Downvote, Báo cáo vi phạm (Report).
- **Module Notification:** Hệ thống thông báo in-app (Core API trước, tích hợp SignalR sau) khi có tương tác bình luận.

## 3. TECH STACK (BACKEND)
- **Framework:** ASP.NET Core 10.0 (Web API)
- **Ngôn ngữ:** C#
- **Database:** PostgreSQL (dựa trên các kiểu dữ liệu ENUM và UUID trong file SQL)
- **ORM:** Entity Framework Core (Code First)
- **Kiến trúc:** Layered Architecture (Controllers, Services, Repositories/Interfaces, Entities, DTOs)
- **Lưu trữ tĩnh (Media):** Cloudinary (Avatar, Cover)
- **Real-time (Future Phase):** SignalR