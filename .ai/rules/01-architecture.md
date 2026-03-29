# LUẬT KIẾN TRÚC & CẤU TRÚC DỰ ÁN (01-ARCHITECTURE)

## 1. Project Core
- **Framework:** ASP.NET Core 10.0 LTS (Web API)
- **Language:** C# 14 (or latest)
- **Database & ORM:** MySQL với Entity Framework Core (Code First).
- **Architecture:** Layered Architecture (Monolithic Single Project).
- **Transition Note:** Team có background Spring Boot. Hãy áp dụng tư duy tương đồng: 
  - `@Service` -> Create Interface in `/Interfaces` + Class in `/Services` + DI (`AddScoped`).
  - `@RestController` -> `[ApiController]` in `/Controllers`.
  - `JpaRepository` -> EF Core `DbSet<T>` in `ApplicationDbContext`.

## 2. Folder Structure (Strict Constraint)
Tuyệt đối tuân thủ phân bổ logic vào đúng thư mục:
- `/Controllers`: Chỉ chứa API Endpoints (`[ApiController]`, `[Route]`).
- `/Services`: Triển khai Business logic.
- `/Interfaces`: Chứa tất cả contracts cho Services.
- `/Entities`: POCO classes (đã được generate ở JOB-002, KHÔNG ĐƯỢC SỬA).
- `/DTOs`: Phân tách Request/Response theo module. Không bao giờ trả Entity trực tiếp ra ngoài.
- `/Data`: `ApplicationDbContext` và configurations.
- `/Middlewares`: Global exception handling.
- `/Security`: JWT, Hashing, Authorization Policies.
- `/Extensions`: `IServiceCollection` extensions để đăng ký DI (Giữ Program.cs sạch).
- `/Configurations`: Classes map với `appsettings.json` qua `IOptions<T>`.
- `/Enums`: Các Enum C# map với Database.

## 3. Core Patterns
- **Error Handling:** Tránh rải rác `try-catch`. Throw custom exceptions ở Service và để Global Exception Middleware bắt và format thành JSON.
- **DTO Mapping:** Luôn map Entity sang Response DTO trước khi trả về Controller.