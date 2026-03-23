# JOB-001: Khởi tạo Base Architecture & Database Connection

**Ngày thực hiện:** 2026-03-24  
**Phạm vi:** Nền móng dự án ReviewFilms API, chưa đi vào logic nghiệp vụ hay entity mapping.

---

## 1. Mục tiêu job

JOB-001 nhằm dựng lại phần khởi tạo chuẩn cho dự án ASP.NET Core 10.0 theo đúng `AGENT.md`:

- Đồng bộ namespace về `ReviewFilms.Api.*`
- Tạo cấu trúc thư mục đúng trách nhiệm
- Chuẩn bị cấu hình PostgreSQL trong `appsettings`
- Tạo middleware xử lý exception toàn cục
- Tạo response wrapper cho API success
- Làm sạch `Program.cs` bằng extension methods

Mục tiêu của job này là tạo nền tảng kỹ thuật đủ rõ ràng để các job sau có thể thêm `DbContext`, entity, service và mapping mà không phải dọn lại kiến trúc ban đầu.

---

## 2. Những gì đã thay đổi

### 2.1. `Program.cs` được rút gọn

`Program.cs` hiện chỉ còn vai trò khởi tạo pipeline ở mức cao:

- đăng ký controllers
- đăng ký Swagger
- gắn global exception middleware
- bật Swagger UI
- map controllers

File này đã được làm sạch để tránh nhồi cấu hình trực tiếp vào entry point.

Tham chiếu:
- [Program.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Program.cs:1)

### 2.2. Cấu hình PostgreSQL và logging

Đã cập nhật:

- `appsettings.json`
- `appsettings.Development.json`

Nội dung bổ sung:

- `ConnectionStrings:DefaultConnection` với chuỗi PostgreSQL giả định
- logging level theo namespace `ReviewFilms.Api`

Điểm đáng chú ý:

- Bản production-like giữ `Microsoft.AspNetCore = Warning`
- Bản development tăng mức log lên `Information`/`Debug` để debug dễ hơn

Tham chiếu:
- [appsettings.json](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\appsettings.json:1)
- [appsettings.Development.json](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\appsettings.Development.json:1)

### 2.3. Global exception middleware

Đã tạo `GlobalExceptionMiddleware.cs` trong `/Middlewares`.

Chức năng:

- bắt exception phát sinh trong pipeline
- log lỗi bằng `ILogger`
- map exception sang HTTP status code phù hợp
- trả response JSON thống nhất theo format:

```json
{
  "success": false,
  "message": "...",
  "errors": []
}
```

Những exception được xử lý riêng:

- `ValidationException` -> 400
- `BadHttpRequestException` -> 400
- `ArgumentException` -> 400
- `FormatException` -> 400
- `UnauthorizedAccessException` -> 401
- `KeyNotFoundException` -> 404
- `InvalidOperationException` -> 409
- mặc định -> 500

Middleware này sử dụng constructor injection đúng yêu cầu.

Tham chiếu:
- [GlobalExceptionMiddleware.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Middlewares\GlobalExceptionMiddleware.cs:1)

### 2.4. Api response wrapper

Đã tạo `ApiResponse<T>` trong `/DTOs/Common`.

Mục đích:

- chuẩn hóa mọi response thành công
- tránh trả entity thẳng ra client
- giữ format ổn định cho frontend / consumer

Format dự kiến:

```json
{
  "success": true,
  "data": { ... },
  "message": "..."
}
```

API wrapper hiện có helper:

- `ApiResponse<T>.Ok(T data, string message = "Success")`
- `ApiResponse<T>.Ok(string message = "Success")`

Tham chiếu:
- [ApiResponse.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\DTOs\Common\ApiResponse.cs:1)

### 2.5. Extension methods cho cấu hình

Đã tạo thư mục `/Extensions` và tách cấu hình ra khỏi `Program.cs`.

Hai extension chính:

- `AddApiControllers()`
- `AddApiSwagger()`
- `UseGlobalExceptionMiddleware()`
- `UseApiSwagger()`

Lợi ích:

- `Program.cs` dễ đọc hơn
- có thể mở rộng thêm `AddApplicationServices()`, `AddJwtAuth()` sau này
- giữ đúng tinh thần `AGENT.md`

Tham chiếu:
- [ServiceCollectionExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\ServiceCollectionExtensions.cs:1)
- [WebApplicationExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\WebApplicationExtensions.cs:1)

### 2.6. Namespace đồng bộ

Các file mẫu hiện có đã được chuyển về namespace chuẩn:

- `ReviewFilms.Api.Controllers`
- `ReviewFilms.Api`

Việc này giúp toàn bộ project đi chung một gốc namespace, tránh lệch chuẩn từ giai đoạn đầu.

Tham chiếu:
- [WeatherForecastController.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Controllers\WeatherForecastController.cs:1)
- [WeatherForecast.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\WeatherForecast.cs:1)

---

## 3. Cấu trúc thư mục đã chuẩn bị

Theo `AGENT.md`, các thư mục nền tảng đã có mặt:

- `Controllers`
- `Services`
- `Interfaces`
- `Entities`
- `DTOs`
- `Data`
- `Migrations`
- `Middlewares`
- `Security`
- `Extensions`
- `Configurations`
- `Enums`

Ghi chú:

- Một số thư mục đang chỉ có placeholder `.gitkeep` để giữ cấu trúc.
- Chưa thêm logic trong `Data`, `Services`, `Entities`, `Security` vì JOB-001 chỉ dừng ở nền móng.

---

## 4. Luồng request hiện tại

Sau JOB-001, request HTTP đi theo luồng:

1. `Program.cs` tạo app và gọi extension methods
2. `GlobalExceptionMiddleware` đứng trước pipeline để bắt lỗi toàn cục
3. Controllers xử lý request
4. Validation errors trả về dạng JSON thống nhất
5. Swagger chỉ bật trong môi trường Development

Kết quả là kiến trúc hiện tại đã đủ sạch để bắt đầu thêm API thực tế mà không làm vỡ shape của response.

---

## 5. Kiểm tra kỹ thuật

### 5.1. Build

Đã chạy:

```bash
dotnet build
```

Kết quả:

- build thành công
- `0 Warning(s)`
- `0 Error(s)`

### 5.2. Lưu ý trong quá trình kiểm tra

Trong lần build đầu, file `ReviewFilms.exe` bị khóa bởi process đang chạy. Đã dừng process đó và build lại thành công.

---

## 6. Review code theo file

### `Program.cs`

Đã đạt mục tiêu clean entry point. File này chỉ điều phối pipeline cấp cao, không chứa logic cấu hình rải rác.

### `Extensions/ServiceCollectionExtensions.cs`

Tập trung tốt, nhưng hiện tại mới dừng ở registration cơ bản. Job sau có thể mở rộng thêm:

- database registration
- application services
- swagger auth config

### `Extensions/WebApplicationExtensions.cs`

Đúng mục tiêu tách middleware pipeline ra khỏi `Program.cs`.

### `Middlewares/GlobalExceptionMiddleware.cs`

Cách map exception sang status code rõ ràng và dễ mở rộng. Format JSON thống nhất phù hợp với yêu cầu backend API enterprise.

### `DTOs/Common/ApiResponse.cs`

Đủ gọn, dễ dùng, và nhất quán cho success response.

### `appsettings.json` / `appsettings.Development.json`

Đã có connection string giả định đúng PostgreSQL, phù hợp để job sau gắn `DbContext`.

---

## 7. Phạm vi chưa làm

JOB-001 chưa bao gồm:

- `ApplicationDbContext`
- EF Core PostgreSQL registration
- entity classes
- repository pattern
- service/business logic
- authentication/authorization thực tế
- logging nâng cao bằng Serilog

Đây là chủ đích để giữ job này đúng nghĩa là "base architecture".

---

## 8. Kết luận

JOB-001 đã hoàn tất phần nền móng:

- kiến trúc thư mục đúng `AGENT.md`
- namespace đã được chuẩn hóa
- response và error format đã có khung thống nhất
- `Program.cs` đã sạch và có thể mở rộng
- build thành công

Nếu muốn review code nhanh, nên đọc theo thứ tự:

1. `Program.cs`
2. `Extensions/ServiceCollectionExtensions.cs`
3. `Extensions/WebApplicationExtensions.cs`
4. `Middlewares/GlobalExceptionMiddleware.cs`
5. `DTOs/Common/ApiResponse.cs`
6. `appsettings.json`
7. `appsettings.Development.json`

