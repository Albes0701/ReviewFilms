# JOB-001 Code Review Checklist

> Mục tiêu: đọc theo thứ tự ưu tiên để xác nhận foundation đã đúng trước khi đi tiếp sang `DbContext`, entity và service.

## Ưu tiên 1

1. [Program.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Program.cs)
2. [Extensions/ServiceCollectionExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\ServiceCollectionExtensions.cs)
3. [Extensions/WebApplicationExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\WebApplicationExtensions.cs)

## Ưu tiên 2

4. [Middlewares/GlobalExceptionMiddleware.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Middlewares\GlobalExceptionMiddleware.cs)
5. [DTOs/Common/ApiResponse.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\DTOs\Common\ApiResponse.cs)

## Ưu tiên 3

6. [appsettings.json](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\appsettings.json)
7. [appsettings.Development.json](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\appsettings.Development.json)
8. [Controllers/WeatherForecastController.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Controllers\WeatherForecastController.cs)
9. [WeatherForecast.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\WeatherForecast.cs)

## Checklist khi review

- Xác nhận namespace đã đồng bộ thành `ReviewFilms.Api.*`
- Xác nhận `Program.cs` không còn chứa cấu hình rải rác
- Xác nhận middleware trả lỗi JSON thống nhất
- Xác nhận response wrapper success có cấu trúc rõ ràng
- Xác nhận connection string PostgreSQL đã được đặt đúng chỗ
- Xác nhận build vẫn sạch trước khi sang job tiếp theo

