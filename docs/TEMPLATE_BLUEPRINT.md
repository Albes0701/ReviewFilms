# VIBECODE PROJECT BLUEPRINT
**Dự án:** ReviewFilms API

## 1. KIẾN TRÚC TỔNG THỂ (ARCHITECTURE)
- **Mô hình:** Monolithic Layered Architecture (Kiến trúc phân tầng nguyên khối).
- **Luồng dữ liệu (Data Flow):** HTTP Request → `[ApiController]` → `IService` (Business Logic) → EF Core `DbContext` → PostgreSQL.

## 2. QUY HOẠCH THƯ MỤC (DIRECTORY STRUCTURE)
Bắt buộc tuân thủ cấu trúc sau để phân tách trách nhiệm rõ ràng:
- `/Controllers`: Chứa các API Endpoints định tuyến HTTP request.
- `/Services`: Chứa class triển khai logic nghiệp vụ thực tế. 
- `/Interfaces`: Chứa các interface (hợp đồng) cho Service (VD: `IFilmService.cs`).
- `/Entities`: Chứa các POCO classes đại diện cho bảng trong Database (tương đương `@Entity` của JPA).
- `/DTOs`: Phân tách Request DTO (nhận data) và Response DTO (trả data), tuyệt đối không trả Entity trực tiếp ra ngoài.
- `/Data`: Chứa `ApplicationDbContext` và các cấu hình Fluent API (nếu cần).
- `/Migrations`: Chứa các file lịch sử thay đổi DB của EF Core (Code-First).
- `/Middlewares`: Chứa pipeline tự chế, đặc biệt là Global Exception Handling.

## 3. PHÂN RÃ MODULES & ENTITIES (Dựa trên SQL Schema)
- **Module Auth & Security:** `user`, `role`, `permission`, `role_permission`, `user_role`, `refresh_token`.
- **Module Film Core:** `movie`, `genres`, `movie_genres`, `persons`, `moviecredits`.
- **Module Review & Social:** `movie_rating`, `comment` (hỗ trợ nested qua `parent_id`, `root_id`), `comment_vote`, `watchlist`, `report`.
- **Module Notification:** `notification`.

## 4. TIÊU CHUẨN KỸ THUẬT (STRICT CONVENTIONS)
- **Dependency Injection (DI):** 100% sử dụng Constructor Injection. Đăng ký Service qua `AddScoped` trong `Program.cs`.
- **Bất đồng bộ (Async/Await):** Mọi thao tác I/O với DB phải dùng `async Task<T>`.
- **Xử lý lỗi (Error Handling):** Dùng Global Exception Middleware để bắt lỗi tập trung, không lạm dụng `try-catch` rải rác trong Service/Controller.
- **Truy vấn DB:** Dùng LINQ Method Syntax thay vì Query Syntax.