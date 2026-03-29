# NGỮ CẢNH MODULE AUTH & SECURITY (JOB-003)

## 1. Trách nhiệm (Role & Scope)
- Bạn phụ trách ĐỘC LẬP module Auth & Security trong kiến trúc Monolithic.
- Các Entity (`User`, `Role`, `RefreshToken`, v.v.) ĐÃ ĐƯỢC TẠO trên MySQL. Tuyệt đối KHÔNG sửa file Entity.

## 2. Luật Đăng ký DI (DI Registration)
- Tạo file `Extensions/AuthModuleExtensions.cs`. 
- Viết extension method `AddAuthModule(this IServiceCollection services, IConfiguration config)` để đăng ký toàn bộ service, cấu hình JWT của module này.

## 3. Yêu cầu Đầu ra (Deliverables)
- **DTOs (`/DTOs/Auth`):** `RegisterRequest`, `LoginRequest`, `AuthResponse`.
- **Security (`/Security`):** - Triển khai `JwtTokenGenerator` (để sinh Access Token).
  - Triển khai `PasswordHasher` (sử dụng thư viện `BCrypt.Net-Next`).
- **Services (`/Services`):** Triển khai `IAuthService` và `AuthService` (Xử lý băm mật khẩu, đăng ký, đăng nhập).
- **Controllers (`/Controllers`):** `AuthController` với các API đăng ký/đăng nhập, trả về format `ApiResponse<T>`.