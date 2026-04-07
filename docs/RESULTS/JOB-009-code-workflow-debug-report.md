# JOB-009 Code Workflow And Debug Report

## 1. Mục đích tài liệu này

Tài liệu này không lặp lại theo kiểu changelog. Mục tiêu là giúp bạn:

- đọc nhanh flow xử lý của code `JOB-009`
- biết class nào chịu trách nhiệm phần nào
- biết dữ liệu đi qua các bước nào trước khi xuống DB
- biết nên đặt breakpoint ở đâu khi có bug nghiệp vụ
- biết những nhánh nào tạo ra `imported`, `skipped`, `failed`

Tài liệu này phù hợp khi bạn cần:

- fix bug import phim từ TMDB
- kiểm tra vì sao bị bỏ qua dữ liệu
- kiểm tra vì sao bị tạo trùng `Person`, `Genre`, `Movie`
- kiểm tra vì sao bulk import chạy không đúng summary

## 2. Bản đồ class và trách nhiệm

## 2.1. `TmdbSyncService`

Vai trò:

- gọi TMDB API
- đọc JSON response
- map response ngoài thành DTO nội bộ để service nghiệp vụ dùng

Không làm:

- không ghi DB
- không check duplicate
- không tạo `Movie`, `Genre`, `Person`, `MovieCredit`

Nói ngắn gọn:

- đây là adapter giữa hệ thống và TMDB

## 2.2. `MovieService`

Vai trò:

- xử lý nghiệp vụ import
- chống trùng dữ liệu
- tạo entity và relation
- chạy transaction khi import 1 phim
- tổng hợp kết quả bulk import

Nói ngắn gọn:

- đây là nơi quyết định import thành công, bỏ qua hay thất bại

## 2.3. `MoviesController`

Vai trò:

- expose endpoint import/sync
- chỉ nhận request, gọi service, trả `ApiResponse<T>`

Không làm:

- không tự gọi TMDB
- không chứa logic nghiệp vụ import

## 2.4. `GenresController` và `PersonsController`

Vai trò:

- expose endpoint read-only
- chỉ load data từ `MovieService`

## 3. Flow tổng thể từ HTTP đến DB

## 3.1. Flow `POST /api/movies/sync-genres`

1. `MoviesController.SyncGenres`
2. gọi `MovieService.SyncGenresAsync`
3. `MovieService` gọi `TmdbSyncService.FetchGenresAsync`
4. TMDB trả danh sách genre
5. `MovieService` normalize `Name` + sinh `Slug`
6. check DB theo `Name` hoặc `Slug`
7. nếu chưa có thì add `Genre`
8. `SaveChangesAsync`
9. trả về số genre mới được thêm

### Điểm breakpoint nên đặt

- đầu `MoviesController.SyncGenres`
- đầu `MovieService.SyncGenresAsync`
- ngay sau `FetchGenresAsync`
- trong nhánh `if (exists)` để xem vì sao genre bị skip

## 3.2. Flow `POST /api/movies/import/single/{tmdbId}`

1. `MoviesController.ImportSingle`
2. gọi `MovieService.ImportMovieFromTmdbAsync(tmdbId)`
3. `MovieService` gọi `TmdbSyncService.FetchMovieDetailsAsync(tmdbId)`
4. `TmdbSyncService` map TMDB payload thành `TmdbMovieDetailsDto`
5. `MovieService` validate `Title`
6. sinh `slug` từ `Title`
7. check `_dbContext.Movies.Any(movie => movie.Slug == slug)`
8. nếu đã tồn tại:
   - trả `IsSuccess = false`
   - message = `"Đã tồn tại"`
   - không throw
9. nếu chưa tồn tại:
   - mở transaction nếu DB provider hỗ trợ
   - đồng bộ genre liên quan bằng `EnsureGenresAsync`
   - tạo `Movie`
   - tạo `MovieGenre`
   - duyệt `Cast` và `Crew`
   - với mỗi person:
     - gọi `EnsurePersonAsync`
     - tạo `MovieCredit`
   - `SaveChangesAsync`
   - commit transaction
   - trả `IsSuccess = true`

### Điểm breakpoint nên đặt

- đầu `ImportMovieFromTmdbAsync`
- sau `FetchMovieDetailsAsync`
- sau khi sinh `slug`
- trong nhánh `if (movieExists)`
- trong `EnsureGenresAsync`
- trong `EnsurePersonAsync`
- trước `SaveChangesAsync`
- trong `catch` rollback nếu import fail

## 3.3. Flow `POST /api/movies/import/bulk`

1. `MoviesController.ImportBulk`
2. gọi `MovieService.ImportBulkPopularMoviesAsync(count)`
3. `MovieService` clamp `count` vào khoảng `1..200`
4. tính số page TMDB cần gọi
5. gọi `TmdbSyncService.FetchPopularMovieIdsAsync(page)` theo từng trang
6. gom tất cả `tmdbId`
7. chạy `foreach` từng `tmdbId`
8. mỗi vòng:
   - gọi `ImportMovieFromTmdbAsync`
   - nếu `IsSuccess = true` thì tăng `ImportedCount`
   - nếu `IsSuccess = false` thì tăng `SkippedCount`
   - nếu throw exception thì tăng `FailedCount`, log warning, rồi chạy tiếp phim sau
9. build `BulkImportResultDto`
10. trả summary

### Điểm breakpoint nên đặt

- đầu `ImportBulkPopularMoviesAsync`
- trong vòng `for` lấy page TMDB
- đầu vòng `foreach`
- nhánh `if (result.IsSuccess)`
- nhánh `else` tăng `SkippedCount`
- `catch (Exception exception)` tăng `FailedCount`
- ngay trước khi build `message`

## 4. Workflow chi tiết từng method quan trọng

## 4.1. `TmdbSyncService.FetchGenresAsync`

Mục đích:

- lấy danh sách genre từ endpoint `/3/genre/movie/list`

Workflow:

1. gọi GET tới TMDB
2. gọi `EnsureSuccessAsync`
3. parse JSON vào `GenreListResponse`
4. nếu `payload?.Genres == null` thì throw `InvalidOperationException`
5. map từng genre thành `TmdbGenreDto`

Bug hay gặp:

- `Tmdb:ApiKey` sai
- `Tmdb:BaseUrl` sai
- JSON từ TMDB bị đổi schema

Triệu chứng:

- lỗi từ `EnsureSuccessAsync`
- trả về message kiểu TMDB request failed

## 4.2. `TmdbSyncService.FetchMovieDetailsAsync`

Mục đích:

- lấy detail 1 phim và cả credits từ `/3/movie/{tmdbId}?append_to_response=credits`

Workflow:

1. gọi GET tới TMDB
2. `EnsureSuccessAsync`
3. parse JSON về `MovieDetailsResponse`
4. map các field:
   - `Title`
   - `OriginalTitle`
   - `Overview`
   - `ReleaseDate`
   - `RuntimeMinutes`
   - `OriginalLanguage`
   - `PosterUrl`
   - `BackdropUrl`
   - `Genres`
   - `Cast`
   - `Crew`

Điểm quan trọng:

- path ảnh từ TMDB được ghép bởi `BuildImageUrl`
- `snake_case` như `original_title`, `release_date`, `poster_path` được map bằng `JsonPropertyName`

Bug hay gặp:

- ảnh null vì `ImageBaseUrl` sai hoặc path null
- `Title` rỗng dẫn tới import fail ở service
- credits không có dữ liệu vì TMDB không trả đủ

## 4.3. `TmdbSyncService.FetchPopularMovieIdsAsync`

Mục đích:

- lấy danh sách `tmdbId` từ endpoint popular theo trang

Workflow:

1. gọi GET `/3/movie/popular?page={page}`
2. `EnsureSuccessAsync`
3. parse `PopularMoviesResponse`
4. lấy `Results.Select(movie => movie.Id)`

Bug hay gặp:

- page trả rỗng
- count lớn nhưng thực tế TMDB không trả đủ item

Triệu chứng:

- `ReviewedCount` thấp hơn kỳ vọng vì số ID gom được ít hơn `count`

## 4.4. `MovieService.SyncGenresAsync`

Mục đích:

- add mới genre thiếu trong DB

Workflow:

1. lấy genres từ TMDB
2. bỏ qua genre có `Name` rỗng
3. normalize `Name`
4. generate `Slug`
5. `AnyAsync` xem DB đã có genre trùng `Name` hoặc `Slug` chưa
6. nếu chưa có thì add
7. cuối cùng chỉ `SaveChangesAsync` nếu có thêm mới

Bug hay gặp:

- có genre tưởng khác nhưng normalize ra cùng slug
- dữ liệu cũ trong DB đã không sạch nên bị skip ngoài ý muốn

Cách debug:

- log hoặc inspect `normalizedName`
- inspect `slug`
- inspect điều kiện `exists`

## 4.5. `MovieService.ImportMovieFromTmdbAsync`

Đây là method quan trọng nhất của JOB-009.

### Nhánh 1: Validate đầu vào TMDB

Nếu `tmdbMovie.Title` rỗng:

- throw `InvalidOperationException`

Nếu slug sinh ra rỗng:

- throw `InvalidOperationException`

Ý nghĩa nghiệp vụ:

- title là điều kiện bắt buộc để import movie

### Nhánh 2: Check duplicate movie

Check:

```csharp
_dbContext.Movies.AnyAsync(movie => movie.Slug == slug)
```

Nếu trùng:

- không throw
- return `TmdbImportResultDto` với:
  - `IsSuccess = false`
  - `Message = "Đã tồn tại"`

Ý nghĩa nghiệp vụ:

- duplicate là trạng thái “skip”, không phải lỗi hệ thống

### Nhánh 3: Import thật

Nếu không duplicate:

1. mở transaction
2. `EnsureGenresAsync`
3. tạo `Movie`
4. tạo các dòng `MovieGenre`
5. tạo `MovieCredit` từ `Cast`
6. tạo `MovieCredit` từ `Crew`
7. `SaveChangesAsync`
8. commit

Nếu lỗi:

- rollback transaction
- throw lại exception để caller xử lý

## 4.6. `MovieService.EnsureGenresAsync`

Mục đích:

- bảo đảm mọi genre của phim đều đã tồn tại trong DB trước khi tạo relation

Workflow:

1. nhận `tmdbGenres`
2. loại genre rỗng tên
3. normalize name + generate slug
4. `DistinctBy(genre => genre.Slug)` để tránh duplicate trong cùng payload
5. query DB tất cả genre có thể match
6. nếu chưa có thì add `Genre` mới
7. trả về list genre để gắn `MovieGenre`

Bug nghiệp vụ có thể xảy ra:

- 2 genre khác nhau từ TMDB nhưng normalize ra cùng slug
- dữ liệu DB chứa genre cũ có `Name` khác format nên match không như mong muốn

## 4.7. `MovieService.EnsurePersonAsync`

Mục đích:

- tái sử dụng `Person` theo `Name`, tránh tạo trùng

Workflow:

1. validate `tmdbPerson.Name`
2. normalize name
3. check `_dbContext.Persons.Local`
4. nếu không có ở local thì check DB
5. nếu vẫn không có thì tạo `Person` mới

Tại sao check `Local` trước:

- trong cùng một lần import có thể một person xuất hiện nhiều lần
- nếu chỉ query DB thì person vừa add nhưng chưa save có thể chưa được match đúng theo mong đợi

Bug nghiệp vụ có thể xảy ra:

- tên người bị khác nhau rất nhẹ, ví dụ khoảng trắng thừa, ký tự đặc biệt, alias
- logic hiện tại chỉ match theo `Name` normalize đơn giản, chưa có alias hoặc fuzzy match

Đây là điểm nên kiểm tra đầu tiên nếu bạn thấy DB vẫn phát sinh `Person` gần giống nhau.

## 4.8. `MovieService.ImportBulkPopularMoviesAsync`

Mục đích:

- import hàng loạt nhưng không để một phim lỗi làm chết cả lô

Workflow:

1. `count = Math.Clamp(count, 1, 200)`
2. tính page cần fetch
3. fetch tất cả ID
4. lặp từng ID
5. mỗi ID:
   - try import
   - success => `ImportedCount++`
   - duplicate => `SkippedCount++`
   - exception => `FailedCount++` và log warning
6. build message tổng hợp

Điểm cần hiểu rõ:

- `SkippedCount` là duplicate nghiệp vụ
- `FailedCount` là lỗi cứng trong quá trình import
- 2 trạng thái này khác nhau

Nếu summary lệch:

- đặt breakpoint trong từng nhánh tăng count
- đối chiếu `ImportMovieFromTmdbAsync` trả về gì hoặc throw gì

## 5. Các điểm dữ liệu dễ phát sinh bug nghiệp vụ

## 5.1. Slug movie

Movie duplicate được xác định bằng slug sinh từ title.

Hệ quả:

- 2 phim khác năm nhưng cùng title vẫn bị coi là duplicate
- đây là đúng theo spec hiện tại vì không dùng `tmdb_id`

Nếu nghiệp vụ sau này muốn phân biệt:

- phải đổi rule duplicate
- hiện tại không nên sửa nếu chưa đổi spec

## 5.2. Person duplicate

Rule hiện tại:

- duplicate nếu `Name.Trim().ToLowerInvariant()` bằng nhau

Hệ quả:

- `"Keanu Reeves"` và `"Keanu Reeves "` là 1 người
- `"Robert Downey Jr."` và `"Robert Downey Jr"` có thể thành 2 người vì normalize hiện tại không bỏ dấu chấm

Nếu bug thuộc nhóm “person bị tạo trùng hơi khác format”, đây là chỗ cần nâng cấp normalize.

## 5.3. Genre duplicate

Genre dùng cả `Name` normalize và `Slug`.

Hệ quả:

- thường ổn hơn `Person`
- nhưng vẫn có thể gặp conflict nếu TMDB đổi tên genre theo format lạ

## 5.4. Bulk count

`count` được clamp `1..200`.

Hệ quả:

- nếu client gửi 500 thì code vẫn chỉ xử lý 200
- nếu cần import nhiều hơn phải gọi nhiều lần

Nếu người dùng báo “gửi 500 nhưng chỉ import ít hơn”, đây là hành vi đúng hiện tại.

## 6. Gợi ý debug theo triệu chứng

## 6.1. Triệu chứng: import single trả `"Đã tồn tại"` nhưng business nghĩ chưa tồn tại

Kiểm tra:

1. title từ TMDB là gì
2. slug generate ra là gì
3. DB đang có movie nào cùng slug

Breakpoint:

- đầu `ImportMovieFromTmdbAsync`
- sau `var slug = NormalizeSlug(tmdbMovie.Title);`
- nhánh `if (movieExists)`

## 6.2. Triệu chứng: import phim thành công nhưng thiếu cast hoặc crew

Kiểm tra:

1. `TmdbSyncService.FetchMovieDetailsAsync` có parse được `Credits` không
2. `payload.Credits.Cast` và `payload.Credits.Crew` có dữ liệu không
3. `EnsurePersonAsync` có throw vì thiếu `Name` không

Breakpoint:

- `FetchMovieDetailsAsync`
- vòng `foreach (var castMember in tmdbMovie.Cast)`
- vòng `foreach (var crewMember in tmdbMovie.Crew)`

## 6.3. Triệu chứng: bulk import dừng sớm hoặc count không đúng

Kiểm tra:

1. `tmdbIds` gom được bao nhiêu
2. `ReviewedCount` tăng ở những vòng nào
3. có exception nào bị catch và tăng `FailedCount` không
4. có `OperationCanceledException` hay không

Breakpoint:

- đầu `ImportBulkPopularMoviesAsync`
- sau mỗi lần `FetchPopularMovieIdsAsync`
- trong `catch (Exception exception)`

## 6.4. Triệu chứng: person bị tạo trùng

Kiểm tra:

1. dữ liệu `Name` từ TMDB của các bản ghi
2. `normalizedName` sau khi qua `NormalizeName`
3. `_dbContext.Persons.Local`
4. query DB theo `Name.ToLower()`

Breakpoint:

- đầu `EnsurePersonAsync`
- sau `var normalizedName`
- sau `localPerson`
- sau query `FirstOrDefaultAsync`

## 6.5. Triệu chứng: genre không được map vào movie

Kiểm tra:

1. `tmdbMovie.Genres` có rỗng không
2. `EnsureGenresAsync` có trả list rỗng không
3. đoạn tạo `MovieGenre` có chạy không

Breakpoint:

- đầu `EnsureGenresAsync`
- `foreach (var genre in genres)` trong `ImportMovieFromTmdbAsync`

## 7. Các test đang bảo vệ logic nghiệp vụ nào

File test quan trọng nhất là `ReviewFilms.Tests/MovieServiceTests.cs`.

Các case bảo vệ:

- `SyncGenresAsync_adds_only_missing_genres`
  - bảo vệ idempotency khi sync genre
- `ImportMovieFromTmdbAsync_returns_duplicate_result_when_slug_already_exists`
  - bảo vệ nhánh skip duplicate movie
- `ImportMovieFromTmdbAsync_reuses_existing_person_by_name`
  - bảo vệ việc reuse `Person`
- `ImportBulkPopularMoviesAsync_continues_when_one_movie_fails`
  - bảo vệ batch không crash vì 1 phim lỗi

Khi sửa bug nghiệp vụ ở JOB-009:

- nên viết thêm test vào file này trước
- vì file này đã là “bản đồ behavior” của toàn module import

## 8. Những điểm hiện tại là chủ đích, không phải bug

- duplicate movie dùng `Slug`, không dùng `tmdb_id`
- duplicate single import trả `IsSuccess = false`, không throw
- bulk import tách `SkippedCount` và `FailedCount`
- `count` bị clamp tối đa `200`
- route read-only là `/api/genres` và `/api/persons`, không nằm dưới `/api/movies`

Nếu bạn thấy hệ thống chạy đúng các hành vi trên thì đó là expected behavior của JOB-009 hiện tại.

## 9. Checklist ngắn khi cần fix bug

1. Xác định bug nằm ở tầng nào:
   - TMDB fetch
   - mapping DTO
   - duplicate logic
   - transaction/persist
   - controller route/response
2. Đặt breakpoint ở method tương ứng.
3. Kiểm tra input sau normalize, nhất là `slug` và `normalizedName`.
4. Kiểm tra nhánh đang đi vào:
   - success
   - skipped
   - failed
5. Viết test tái hiện bug trước khi sửa.
6. Chỉ sửa rule nghiệp vụ nếu business thật sự muốn đổi spec, đặc biệt ở duplicate movie/person.
