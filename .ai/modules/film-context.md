# NGỮ CẢNH MODULE FILM & MEDIA (JOB-004)

## 1. Trách nhiệm (Role & Scope)
- Bạn phụ trách ĐỘC LẬP module Film & Media trong kiến trúc Monolithic.
- Các Entity (`Movie`, `Genre`, `Person`, `MovieCredit`, v.v.) ĐÃ ĐƯỢC TẠO. Tuyệt đối KHÔNG sửa file Entity.

## 2. Luật Đăng ký DI & Mocking
- **DI Registration:** Tạo file `Extensions/FilmModuleExtensions.cs`. Viết extension method `AddFilmModule(this IServiceCollection services, IConfiguration config)` để đăng ký DI cho module này.
- **Mocking Data:** Nếu API (như tạo phim mới) cần biết `CreatedBy` (Guid của Admin), TẠM THỜI gán cứng (hardcode) một `Guid.NewGuid()` hoặc nhận từ tham số nội bộ. Trách nhiệm lấy User thực tế từ JWT sẽ do team Auth lo khi merge.

## 3. Yêu cầu Đầu ra (Deliverables)
- **DTOs (`/DTOs/Films`):** `MovieDto`, `MovieCreateRequest`, `MovieUpdateRequest`. Cần map khéo léo để tránh vòng lặp JSON khi có relation.
- **Services (`/Services`):** - `ICloudinaryService` (Tích hợp Cloudinary SDK để upload ảnh).
  - `IMovieService` (CRUD phim, thể loại, phân trang).
- **Controllers (`/Controllers`):** `MoviesController` (GET list có phân trang, GET detail, POST, PUT, DELETE) bằng `ApiResponse<T>`.
- **Cấu hình:** Cung cấp class cấu hình Cloudinary (`CloudinarySettings` mapped bằng `IOptions`).