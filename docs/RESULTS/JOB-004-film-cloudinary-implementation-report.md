# JOB-004: Film Module & Cloudinary Integration Report

**Ngày thực hiện:** 2026-03-30  
**Phạm vi:** Giải thích chi tiết phần triển khai module Film, CRUD phim, phân trang, lọc phim, và tích hợp upload ảnh qua Cloudinary.

---

## 1. Mục tiêu của job

JOB-004 bổ sung module quản lý phim cho ReviewFilms API. Phạm vi triển khai gồm:

- DTO cho phim và mapping từ Entity sang DTO
- Service xử lý CRUD phim, lọc, phân trang
- Service upload ảnh lên Cloudinary
- Controller expose API cho danh sách, chi tiết, tạo mới và cập nhật
- Cấu hình DI và appsettings cho Cloudinary

Điểm quan trọng của job này là:

- Không sửa Entity
- Không trả Entity trực tiếp ra ngoài API
- Khi lấy chi tiết phim phải include dữ liệu liên quan nhưng vẫn trả về DTO phẳng để tránh circular reference
- Dùng async/await và LINQ Method Syntax

---

## 2. Kiến trúc triển khai

Module Film được tách thành các phần đúng theo kiến trúc của dự án:

- `Controllers`: chỉ nhận request và trả response
- `Interfaces`: khai báo contract của service
- `Services`: chứa business logic
- `DTOs/Films`: chứa request/response model cho film
- `Configurations`: chứa cấu hình đọc từ `appsettings.json`
- `Extensions`: đăng ký DI cho module

Luồng xử lý tổng quát:

1. Request đi vào `MoviesController`
2. Controller gọi `IMovieService`
3. `MovieService` làm việc với `ApplicationDbContext`
4. Khi cần upload ảnh, `MovieService` gọi `ICloudinaryService`
5. Kết quả được map về `MovieDto`
6. Controller trả về `ApiResponse<T>`

---

## 3. DTOs của Film

### 3.1 `MovieDto`

File: [DTOs/Films/MovieDto.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\DTOs\Films\MovieDto.cs)

Đây là DTO phản hồi chính cho module phim. Nó chứa các trường cần thiết để hiển thị danh sách hoặc chi tiết phim:

- Thông tin cơ bản: `Id`, `Title`, `Slug`, `Overview`
- Metadata: `ReleaseDate`, `RuntimeMinutes`, `AgeRating`, `OriginalLanguage`
- Ảnh: `PosterUrl`, `BackdropUrl`, `TrailerUrl`
- Thống kê: `AvgRating`, `RatingCount`, `CommentCount`
- Trạng thái: `Status`
- Audit fields: `CreatedAt`, `UpdatedAt`, `CreatedByUserId`
- Quan hệ: `Genres`, `Credits`

Điểm thiết kế quan trọng:

- `MovieDto` không chứa navigation entity như `MovieGenres`, `MovieCredits`, `Genre`, `Person`
- Chỉ chứa DTO con là `MovieGenreDto` và `MovieCreditDto`
- Có helper `FromEntity(Movie movie, bool includeRelations)` để map chuẩn từ Entity

`includeRelations = true` dùng cho màn chi tiết, còn list chỉ lấy dữ liệu phẳng để giảm payload.

### 3.2 `MovieGenreDto`

File: [DTOs/Films/MovieGenreDto.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\DTOs\Films\MovieGenreDto.cs)

Chỉ giữ thông tin cần hiển thị của thể loại:

- `Id`
- `Name`
- `Slug`

### 3.3 `MovieCreditDto`

File: [DTOs/Films/MovieCreditDto.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\DTOs\Films\MovieCreditDto.cs)

Chỉ giữ thông tin cần hiển thị của credit:

- `Id`
- `PersonId`
- `PersonName`
- `PersonOriginalName`
- `CreditType`
- `Department`
- `Job`
- `CharacterName`
- `BillingOrder`

### 3.4 `MovieCreateRequest` và `MovieUpdateRequest`

Files:

- [MovieCreateRequest.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\DTOs\Films\MovieCreateRequest.cs)
- [MovieUpdateRequest.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\DTOs\Films\MovieUpdateRequest.cs)

Hai request này dùng cho `multipart/form-data` để hỗ trợ upload file.

`MovieCreateRequest` chứa:

- `Title`
- `OriginalTitle`
- `Slug`
- `Overview`
- `ReleaseDate`
- `RuntimeMinutes`
- `AgeRating`
- `OriginalLanguage`
- `TrailerUrl`
- `Status`
- `PosterFile`
- `BackdropFile`
- `GenreIds`

`MovieUpdateRequest` tương tự nhưng cho phép nullable nhiều hơn để update từng phần.

Thiết kế này cho phép:

- tạo phim kèm upload ảnh
- cập nhật ảnh poster/backdrop riêng
- gán nhiều thể loại qua `GenreIds`

---

## 4. Cloudinary

### 4.1 `CloudinarySettings`

File: [Configurations/CloudinarySettings.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Configurations\CloudinarySettings.cs)

Class này map với section `Cloudinary` trong appsettings:

- `CloudName`
- `UploadPreset`
- `DefaultFolder`

`SectionName = "Cloudinary"` giúp module đọc config thống nhất qua `IOptions<CloudinarySettings>`.

### 4.2 `ICloudinaryService`

File: [Interfaces/ICloudinaryService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Interfaces\ICloudinaryService.cs)

Contract này định nghĩa một API:

- upload ảnh từ `IFormFile`
- trả về URL ảnh đã upload

Method:

- `UploadImageAsync(IFormFile? file, string? folder = null, CancellationToken cancellationToken = default)`

### 4.3 `CloudinaryService`

File: [Services/CloudinaryService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Services\CloudinaryService.cs)

Service này dùng `HttpClient` để gọi trực tiếp Cloudinary REST API.

Luồng xử lý:

1. Kiểm tra file rỗng
2. Validate config `CloudName` và `UploadPreset`
3. Tạo `MultipartFormDataContent`
4. Đẩy `file`
5. Đẩy `upload_preset`
6. Nếu có folder thì đẩy thêm `folder`
7. Gửi request tới:

```text
https://api.cloudinary.com/v1_1/{CloudName}/image/upload
```

8. Đọc JSON response
9. Lấy `secure_url` hoặc fallback `url`

Thiết kế này có vài lợi ích:

- không cần thêm package Cloudinary SDK
- đơn giản, dễ test, ít phụ thuộc
- phù hợp yêu cầu trả về URL trực tiếp

Nếu upload thất bại, service ném `InvalidOperationException`, để global exception middleware trả lỗi chuẩn JSON.

---

## 5. Movie Service

### 5.1 `IMovieService`

File: [Interfaces/IMovieService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Interfaces\IMovieService.cs)

Contract gồm 4 nhóm chức năng:

- lấy danh sách phim có phân trang
- lấy phim chi tiết theo id
- tạo phim
- cập nhật phim

### 5.2 `MovieService`

File: [Services/MovieService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Services\MovieService.cs)

Đây là service chính của module Film.

#### 5.2.1 `GetMoviesAsync`

Mục tiêu:

- phân trang
- filter theo từ khóa
- filter theo `GenreId`
- filter theo `Status`

Query được xây dựng theo cách incremental:

- bắt đầu từ `_dbContext.Movies.AsNoTracking()`
- thêm `Where` nếu có `search`
- thêm `Where` nếu có `genreId`
- thêm `Where` nếu có `status`
- đếm tổng số bản ghi
- `OrderByDescending`
- `Skip` và `Take`
- `Select` sang `MovieDto`

Điểm quan trọng:

- dùng LINQ Method Syntax
- không trả entity
- list view chỉ lấy field cần thiết
- giảm payload và tránh vòng lặp JSON

Kết quả trả về là `PagedResult<MovieDto>`.

#### 5.2.2 `GetMovieByIdAsync`

Mục tiêu:

- lấy chi tiết một phim
- include `MovieGenres -> Genre`
- include `MovieCredits -> Person`
- trả về DTO, không trả entity

Query dùng:

- `AsNoTracking()`
- `AsSplitQuery()`
- `Include(...).ThenInclude(...)`

`AsSplitQuery()` giúp giảm nguy cơ Cartesian explosion khi load nhiều collection navigation.

Sau khi lấy entity, service gọi:

- `MovieDto.FromEntity(movie, includeRelations: true)`

#### 5.2.3 `CreateMovieAsync`

Mục tiêu:

- tạo bản ghi `Movie`
- upload poster/backdrop nếu có
- gán thể loại qua `GenreIds`
- sinh slug duy nhất

Các bước chính:

1. Trim title và slug input
2. Sinh slug chuẩn hóa bằng `NormalizeSlug`
3. Kiểm tra slug trùng và thêm suffix nếu cần
4. Load genres theo `GenreIds`
5. Upload ảnh poster/backdrop qua `ICloudinaryService`
6. Tạo entity `Movie`
7. Tạo list `MovieGenre`
8. Save changes
9. Load lại phim mới tạo bằng `GetMovieByIdAsync`

Lý do load lại sau khi save:

- đảm bảo response có đủ relation đã include
- giữ một luồng mapping duy nhất thông qua `MovieDto`

#### 5.2.4 `UpdateMovieAsync`

Mục tiêu:

- cập nhật các field của phim
- đổi slug nếu title hoặc slug thay đổi
- upload ảnh mới nếu client gửi file
- cập nhật danh sách genre nếu có `GenreIds`

Chiến lược update:

- chỉ update field nào client thực sự gửi
- nếu `request.Slug` có giá trị thì ưu tiên nó
- nếu không có slug nhưng có đổi title thì regenerate slug từ title
- nếu file mới được upload thành công thì thay URL cũ
- nếu `GenreIds` được truyền vào thì replace toàn bộ danh sách genre hiện tại

### 5.3 Helper methods

#### `LoadGenresAsync`

Load danh sách genre bằng `genreIds.Contains(genre.Id)`.

Nếu thiếu genre nào, service ném `KeyNotFoundException` và liệt kê id còn thiếu.

#### `GenerateUniqueSlugAsync`

Mục tiêu:

- tạo slug an toàn, chuẩn hóa
- tránh trùng với slug đã tồn tại

Quy trình:

1. Chuẩn hóa input bằng regex
2. Tìm các slug đã tồn tại có cùng base
3. Nếu chưa trùng thì trả base slug
4. Nếu trùng thì tìm suffix lớn nhất rồi cộng thêm 1

Ví dụ:

- `interstellar`
- `interstellar-2`
- `interstellar-3`

#### `NormalizeSlug`

Chuyển text thành slug:

- lowercase
- bỏ ký tự đặc biệt
- thay khoảng trắng bằng `-`

---

## 6. Controller

### `MoviesController`

File: [Controllers/MoviesController.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Controllers\MoviesController.cs)

Controller này chỉ làm nhiệm vụ:

- nhận request
- gọi service
- trả `ApiResponse<T>`

Endpoints:

- `GET /api/movies`
- `GET /api/movies/{id}`
- `POST /api/movies`
- `PUT /api/movies/{id}`

#### `GET /api/movies`

Nhận query:

- `pageNumber`
- `pageSize`
- `search`
- `genreId`
- `status`

Trả:

- `ApiResponse<PagedResult<MovieDto>>`

#### `GET /api/movies/{id}`

Trả:

- `ApiResponse<MovieDto>`

#### `POST /api/movies`

Nhận `[FromForm] MovieCreateRequest`

Trả:

- `201 Created`
- body là `ApiResponse<MovieDto>`

#### `PUT /api/movies/{id}`

Nhận `[FromForm] MovieUpdateRequest`

Trả:

- `ApiResponse<MovieDto>`

---

## 7. Paging model

### `PagedResult<T>`

File: [DTOs/Common/PagedResult.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\DTOs\Common\PagedResult.cs)

Mục tiêu là chuẩn hóa dữ liệu phân trang cho frontend.

Fields:

- `Items`
- `PageNumber`
- `PageSize`
- `TotalCount`
- `TotalPages`
- `HasPreviousPage`
- `HasNextPage`

Thiết kế này đủ để frontend render:

- pagination bar
- tổng số bản ghi
- trạng thái trang hiện tại

---

## 8. Dependency Injection

### `FilmModuleExtensions`

File: [Extensions/FilmModuleExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Extensions\FilmModuleExtensions.cs)

Extension này gom toàn bộ DI của module Film:

- `Configure<CloudinarySettings>`
- `AddHttpClient<ICloudinaryService, CloudinaryService>()`
- `AddScoped<IMovieService, MovieService>()`

### Nối vào nền hiện tại

File: [Extensions/ServiceCollectionExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\films-media\Extensions\ServiceCollectionExtensions.cs)

`AddFilmModule(configuration)` được gọi từ `AddApplicationDbContext(...)` để:

- không sửa `Program.cs`
- giữ startup sạch
- phù hợp rule của repo về module registration

---

## 9. appsettings

### `appsettings.json`

Đã thêm section:

```json
"Cloudinary": {
  "CloudName": "your-cloud-name",
  "UploadPreset": "your-upload-preset",
  "DefaultFolder": "reviewfilms"
}
```

### `appsettings.Development.json`

Thêm tương tự, nhưng folder mặc định khác:

```json
"Cloudinary": {
  "CloudName": "your-cloud-name",
  "UploadPreset": "your-upload-preset",
  "DefaultFolder": "reviewfilms-dev"
}
```

Lưu ý:

- `CloudName` và `UploadPreset` là placeholder
- khi tích hợp thật, cần thay bằng giá trị thực từ Cloudinary dashboard

---

## 10. JSON circular reference

Yêu cầu của job là không bị circular reference khi lấy chi tiết phim.

Giải pháp đã dùng:

- controller không trả entity
- service lấy entity rồi map sang DTO
- DTO chỉ chứa dữ liệu cần thiết
- collection navigation của EF không đi thẳng ra ngoài API

Điều này loại bỏ nguy cơ:

- `Movie -> MovieGenres -> Movie -> ...`
- `Movie -> MovieCredits -> Person -> MovieCredits -> ...`

---

## 11. Error handling

Module Film không tự nuốt exception.

Các lỗi phổ biến được ném ra từ service:

- `KeyNotFoundException` nếu không tìm thấy movie hoặc genre
- `ArgumentException` nếu slug rỗng sau chuẩn hóa
- `InvalidOperationException` nếu Cloudinary upload thất bại hoặc config thiếu

Global exception middleware đã có sẵn trong nền dự án sẽ format lỗi thành JSON thống nhất.

---

## 12. Những điểm cần lưu ý khi tích hợp thật

1. `POST` và `PUT` đang dùng `multipart/form-data`, frontend phải gửi đúng kiểu này.
2. `Cloudinary` hiện dùng `upload_preset` unsigned. Nếu môi trường production dùng signed upload thì cần thay chiến lược xác thực.
3. `CreatedByUserId` hiện đang để `null` vì job này chưa tích hợp Auth/JWT.
4. `GenreIds` khi update nếu truyền vào sẽ replace toàn bộ danh sách genre hiện tại.
5. Các query hiện ưu tiên an toàn và rõ ràng, chưa tối ưu sâu cho truy vấn rất lớn.

---

## 13. Kết quả build

Sau khi triển khai:

- `dotnet build ReviewFilms.csproj --no-restore`
- Kết quả: thành công

Điều đó xác nhận:

- namespace hợp lệ
- DI và controller compile được
- DTO và service không có lỗi cú pháp

---

## 14. Tóm tắt ngắn

JOB-004 đã hoàn thành một module Film độc lập với các đặc điểm:

- API CRUD phim rõ ràng
- phân trang và filter
- mapping DTO an toàn
- upload ảnh Cloudinary trả URL
- không sửa Entity
- không đụng `Program.cs`
- tuân thủ kiến trúc layered của dự án

