# JOB-009 TMDB Sync Implementation Report

## 1. Phạm vi đã triển khai

JOB-009 đã được triển khai theo hướng tách:

- `TmdbSyncService`: chỉ chịu trách nhiệm gọi TMDB API và map payload ngoài thành DTO nội bộ.
- `MovieService`: chịu trách nhiệm đồng bộ genre, import một phim, import hàng loạt, chống trùng lặp, tạo `Person`, `MovieCredit`, `MovieGenre`, và trả kết quả tổng hợp.

Các endpoint đã bổ sung:

- `POST /api/movies/sync-genres`
- `POST /api/movies/import/single/{tmdbId}`
- `POST /api/movies/import/bulk`
- `GET /api/genres`
- `GET /api/persons`

## 2. File chính đã thêm/cập nhật

### File mới

- `Configurations/TmdbSettings.cs`
- `Interfaces/ITmdbSyncService.cs`
- `Services/TmdbSyncService.cs`
- `Controllers/GenresController.cs`
- `Controllers/PersonsController.cs`
- `DTOs/Films/GenreListItemDto.cs`
- `DTOs/Films/PersonListItemDto.cs`
- `DTOs/Films/TmdbImportResultDto.cs`
- `DTOs/Films/BulkImportRequest.cs`
- `DTOs/Films/BulkImportResultDto.cs`
- `DTOs/Films/TmdbGenreDto.cs`
- `DTOs/Films/TmdbMovieDetailsDto.cs`
- `DTOs/Films/TmdbMovieCreditPersonDto.cs`
- `ReviewFilms.Tests/MoviesControllerSyncTests.cs`
- `docs/superpowers/plans/2026-04-03-job-009-tmdb-sync.md`

### File cập nhật

- `Services/MovieService.cs`
- `Interfaces/IMovieService.cs`
- `Controllers/MoviesController.cs`
- `Extensions/FilmModuleExtensions.cs`
- `ReviewFilms.Tests/MovieServiceTests.cs`
- `appsettings.json`
- `appsettings.Development.json`

## 3. Luồng xử lý Bulk Import

### Bước 1: Lấy danh sách `tmdbId`

`ImportBulkPopularMoviesAsync(int count)` tính số trang cần gọi theo công thức:

- mỗi trang TMDB popular có tối đa 20 phim
- `requiredPages = ceil(count / 20)`

Sau đó service gọi `FetchPopularMovieIdsAsync(page)` theo từng trang và gom toàn bộ `tmdbId` đến khi đủ `count`.

### Bước 2: Duyệt từng phim bằng `foreach`

Service dùng vòng lặp `foreach` trên danh sách `tmdbId` đã gom được.

Với mỗi `tmdbId`:

1. gọi `ImportMovieFromTmdbAsync(tmdbId)`
2. nếu kết quả `IsSuccess = true` thì tăng `ImportedCount`
3. nếu kết quả `IsSuccess = false` thì tăng `SkippedCount`
4. nếu phát sinh exception thì:
   - tăng `FailedCount`
   - ghi log warning
   - tiếp tục vòng lặp với phim kế tiếp

### Bước 3: Trả về summary

Kết quả cuối cùng trả về:

- `RequestedCount`
- `ReviewedCount`
- `ImportedCount`
- `SkippedCount`
- `FailedCount`
- `Message`

Ví dụ message:

- không có lỗi cứng:
  - `Đã duyệt X phim, Import thành công Y phim, Bỏ qua Z phim trùng lặp.`
- có phim lỗi:
  - `Đã duyệt X phim, Import thành công Y phim, Bỏ qua Z phim trùng lặp, Lỗi W phim.`

## 4. Cơ chế bỏ qua dữ liệu trùng

### Movie

Không dùng `tmdb_id`.

Movie được chống trùng bằng:

- tạo slug từ `Title`
- check `_dbContext.Movies.Any(movie => movie.Slug == slug)`

Nếu đã tồn tại:

- không throw exception
- trả `TmdbImportResultDto { IsSuccess = false, Message = "Đã tồn tại" }`

Điều này làm cho import single và bulk đều có tính idempotent ở mức movie.

### Genre

Genre được chống trùng bằng:

- `Name` normalize về lowercase + trim
- `Slug` normalize từ tên

`SyncGenresAsync()` chỉ add genre mới nếu DB chưa có genre trùng theo `Name` hoặc `Slug`.

### Person

Person được chống trùng bằng:

- `Name` normalize về lowercase + trim

Khi import credit:

1. check `DbContext.Persons.Local`
2. nếu chưa có thì check DB theo `Name`
3. chỉ tạo `Person` mới nếu cả local cache lẫn DB đều không có

Nhờ đó:

- không tạo trùng `Person` khi một người xuất hiện nhiều lần trong cùng một phim
- không tạo trùng `Person` giữa các lần import khác nhau

## 5. Cơ chế chống crash trong Bulk Import

Điểm quan trọng của JOB-009 là:

- một phim hỏng dữ liệu từ TMDB không được làm văng toàn bộ batch

Triển khai hiện tại xử lý như sau:

- `ImportMovieFromTmdbAsync` vẫn được phép throw nếu payload không hợp lệ, ví dụ thiếu `Title`
- `ImportBulkPopularMoviesAsync` bọc mỗi lần import trong `try/catch`
- khi một phim lỗi:
  - batch không dừng
  - service log warning
  - batch tiếp tục với phim tiếp theo
  - `FailedCount` tăng lên

Ngoại lệ duy nhất không bị nuốt là `OperationCanceledException`, để việc hủy request vẫn hoạt động đúng.

## 6. Transaction trong single import

`ImportMovieFromTmdbAsync` chạy trong transaction khi provider hỗ trợ relational transaction.

Mục đích:

- đảm bảo movie, genres mapping, persons, credits được tạo đồng bộ
- nếu lỗi giữa chừng thì rollback, tránh dữ liệu nửa vời

Với test in-memory, service tự bỏ qua bước mở transaction thực để không làm hỏng unit test.

## 7. TDD và test coverage

Các test mới đã cover các hành vi chính:

- `SyncGenresAsync_adds_only_missing_genres`
- `ImportMovieFromTmdbAsync_returns_duplicate_result_when_slug_already_exists`
- `ImportMovieFromTmdbAsync_reuses_existing_person_by_name`
- `ImportBulkPopularMoviesAsync_continues_when_one_movie_fails`
- `MoviesController_exposes_authorized_sync_and_import_endpoints`
- `Read_only_lookup_controllers_expose_expected_routes`

## 8. Verify đã chạy

Lệnh verify:

```powershell
dotnet test ReviewFilms.Tests/ReviewFilms.Tests.csproj
```

Kết quả tại thời điểm hoàn tất:

- Passed: `19`
- Failed: `0`

## 9. Ràng buộc đã tuân thủ

- Không sửa file trong thư mục `Entities`
- Không sửa `Program.cs`
- Không dùng `tmdb_id` để lưu hay đối sánh
- Chống trùng lặp bằng `Slug` của movie và `Name`/`Slug` của dữ liệu liên quan
