# JOB-005 Code Explanation

**Ngày thực hiện:** 2026-03-30  
**Mục tiêu:** Giải thích chi tiết code của module Review (Rating & Nested Comments), bao gồm DTOs, service, controller, DI và thuật toán dựng cây comment.

---

## 1. Tổng quan module

JOB-005 triển khai một module review độc lập cho ReviewFilms API. Mục tiêu của module là:

- cho phép user đã đăng nhập đánh giá phim theo thang điểm `1..10`
- cho phép user tạo comment đa cấp bằng `ParentId` và `RootId`
- trả về cây comment mà không query đệ quy xuống DB theo từng node
- cập nhật lại `Movie.AvgRating` và `Movie.RatingCount` sau mỗi thay đổi rating

Module này tuân thủ kiến trúc layered của dự án:

- Controller chỉ nhận request và trả response
- Service chứa business logic
- Interface tách contract khỏi implementation
- DTO tách request/response ra khỏi entity
- Entity không bị sửa đổi

---

## 2. Danh sách file chính

- [Controllers/ReviewsController.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Controllers/ReviewsController.cs)
- [Services/ReviewService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Services/ReviewService.cs)
- [Services/CurrentUserService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Services/CurrentUserService.cs)
- [Interfaces/IReviewService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Interfaces/IReviewService.cs)
- [Interfaces/ICurrentUserService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Interfaces/ICurrentUserService.cs)
- [DTOs/Reviews/RatingRequest.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/DTOs/Reviews/RatingRequest.cs)
- [DTOs/Reviews/CommentRequest.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/DTOs/Reviews/CommentRequest.cs)
- [DTOs/Reviews/CommentResponse.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/DTOs/Reviews/CommentResponse.cs)
- [Extensions/ReviewModuleExtensions.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Extensions/ReviewModuleExtensions.cs)
- [Extensions/ServiceCollectionExtensions.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Extensions/ServiceCollectionExtensions.cs)

---

## 3. DTOs

### 3.1 `RatingRequest`

File: [DTOs/Reviews/RatingRequest.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/DTOs/Reviews/RatingRequest.cs)

DTO này dùng cho request đánh giá phim.

Nội dung:

- `MovieId`: phim cần rating
- `Score`: điểm từ `1` đến `10`

Ràng buộc:

- `[Required]` cho `MovieId`
- `[Range(1, 10)]` cho `Score`

Ý nghĩa:

- bảo vệ đầu vào ở tầng API
- tránh user gửi điểm ngoài domain cho phép
- giúp controller không phải tự validate từng field thủ công

### 3.2 `CommentRequest`

File: [DTOs/Reviews/CommentRequest.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/DTOs/Reviews/CommentRequest.cs)

DTO này dùng cho request tạo comment.

Nội dung:

- `MovieId`: phim đang comment
- `Content`: nội dung comment
- `ParentId`: comment cha, nullable

Ràng buộc:

- `[Required]` cho `MovieId`
- `[Required]` và `[StringLength(4000, MinimumLength = 1)]` cho `Content`

Ý nghĩa của `ParentId`:

- nếu `ParentId = null`, comment là comment gốc
- nếu `ParentId` có giá trị, comment là reply của comment đó

### 3.3 `CommentResponse`

File: [DTOs/Reviews/CommentResponse.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/DTOs/Reviews/CommentResponse.cs)

DTO này dùng cho response comment.

Nội dung chính:

- thông tin định danh: `Id`, `MovieId`, `UserId`
- thông tin cây: `ParentId`, `RootId`, `Depth`
- thông tin nội dung: `Content`, `IsEdited`, `EditedAt`, `DeletedAt`
- thông tin vote/reply: `Score`, `UpvoteCount`, `DownvoteCount`, `ReplyCount`
- danh sách con: `ChildComments`

Điểm quan trọng:

- `ChildComments` là `List<CommentResponse>`
- DTO này có thể biểu diễn cả flat node lẫn cây comment
- cây được xây trong memory, không cần query con lặp lại

---

## 4. Interface layer

### 4.1 `IReviewService`

File: [Interfaces/IReviewService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Interfaces/IReviewService.cs)

Interface này gom toàn bộ nghiệp vụ review.

Method:

- `UpsertRatingAsync(...)`
- `DeleteRatingAsync(...)`
- `CreateCommentAsync(...)`
- `GetCommentsAsync(...)`

Ý nghĩa thiết kế:

- controller phụ thuộc vào interface, không phụ thuộc class cụ thể
- việc test service hoặc thay implementation sau này dễ hơn
- giữ đúng layered architecture của dự án

### 4.2 `ICurrentUserService`

File: [Interfaces/ICurrentUserService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Interfaces/ICurrentUserService.cs)

Interface này trừu tượng hóa việc lấy `UserId` hiện tại từ JWT claims.

Method:

- `GetCurrentUserId()`

Ý nghĩa:

- controller không phải tự parse claims
- logic lấy user hiện tại được gom về một chỗ
- sau này nếu team thay cách đọc claims thì chỉ cần đổi implementation

---

## 5. Service layer

### 5.1 `CurrentUserService`

File: [Services/CurrentUserService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Services/CurrentUserService.cs)

Nhiệm vụ:

- đọc `HttpContext.User`
- lấy claim chứa user id
- parse sang `Guid`
- throw lỗi nếu user chưa đăng nhập hoặc claim không hợp lệ

Luồng xử lý:

- kiểm tra `Identity.IsAuthenticated`
- đọc `ClaimTypes.NameIdentifier`
- fallback sang `sub`
- fallback sang `nameid`
- parse `Guid`

Tại sao làm vậy:

- nhiều hệ JWT dùng claim khác nhau cho user id
- `NameIdentifier` là chuẩn phổ biến của ASP.NET Core
- `sub` thường gặp trong JWT theo chuẩn OpenID/JWT

Nếu không tìm thấy user id:

- service throw `UnauthorizedAccessException`
- middleware lỗi toàn cục sẽ format thành JSON thống nhất

### 5.2 `ReviewService`

File: [Services/ReviewService.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Services/ReviewService.cs)

Đây là class chứa toàn bộ business logic của JOB-005.

#### a. `UpsertRatingAsync`

Mục tiêu:

- thêm rating mới nếu user chưa rate
- cập nhật rating cũ nếu user đã rate
- tính lại `Movie.AvgRating` và `Movie.RatingCount`

Luồng:

- validate score từ `1..10`
- mở transaction
- load movie theo `movieId`
- tìm `MovieRating` theo `(MovieId, UserId)`
- nếu chưa có rating:
  - tạo record mới
  - gán `Id`, `CreatedAt`, `UpdatedAt`
- nếu đã có:
  - cập nhật `Score`
  - cập nhật `UpdatedAt`
- `SaveChangesAsync`
- tính lại aggregate rating của movie
- `SaveChangesAsync` lần nữa
- commit transaction

Lý do dùng transaction:

- bảo đảm rating và aggregate movie luôn đồng bộ
- tránh trạng thái trung gian nếu request bị lỗi giữa chừng

#### b. `DeleteRatingAsync`

Mục tiêu:

- xóa rating của user trên một movie
- tính lại aggregate movie sau khi xóa

Luồng:

- mở transaction
- load movie
- tìm rating theo `(MovieId, UserId)`
- nếu không có rating thì throw `KeyNotFoundException`
- xóa rating
- lưu thay đổi
- tính lại `AvgRating` và `RatingCount`
- commit

#### c. `CreateCommentAsync`

Mục tiêu:

- tạo comment gốc hoặc reply
- xử lý đúng `ParentId` và `RootId`
- cập nhật `ReplyCount` cho node liên quan

Luồng:

- validate content không rỗng
- load movie
- tạo comment mới với trạng thái mặc định:
  - `Depth = 0`
  - `Score = 0`
  - `UpvoteCount = 0`
  - `DownvoteCount = 0`
  - `ReplyCount = 0`
  - `Status = CommentStatus.Visible`
- nếu `ParentId` có giá trị:
  - tìm comment cha
  - kiểm tra comment cha tồn tại
  - kiểm tra comment cha đang `Visible`
  - gán `ParentId`
  - gán `RootId = parent.RootId ?? parent.Id`
  - tăng `Depth = parent.Depth + 1`
  - tăng `ReplyCount` cho parent
  - nếu root khác parent thì tăng thêm `ReplyCount` cho root
- nếu không có `ParentId`:
  - `RootId = Id` của chính comment mới
- tăng `Movie.CommentCount`
- thêm comment vào DB
- lưu thay đổi
- map sang `CommentResponse`

Ý nghĩa `ParentId` và `RootId`:

- `ParentId` mô tả comment ngay phía trên mà reply bám vào
- `RootId` xác định thread gốc của toàn bộ nhánh

Điểm lưu ý:

- code hiện tại giữ cấu trúc tree ở mức entity sẵn có
- `ReplyCount` được cập nhật để hỗ trợ hiển thị nhanh số reply

#### d. `GetCommentsAsync`

Mục tiêu:

- lấy danh sách comment của một movie
- trả về cây nested comments
- tránh N+1 query

Luồng:

- kiểm tra movie tồn tại
- query một lần toàn bộ comment `Visible` của movie
- `AsNoTracking()` để giảm overhead EF
- `Select(...)` sang `CommentResponse`
- khởi tạo `ChildComments = new List<CommentResponse>()`
- gọi `BuildCommentTree(...)`

Tại sao đây là cách tối ưu:

- chỉ một query để lấy toàn bộ node
- không query con theo từng comment
- cây được lắp trong memory bằng dictionary lookup

#### e. `RecalculateMovieRatingAsync`

Mục tiêu:

- tính lại rating trung bình của movie sau mỗi thao tác rating

Luồng:

- đếm số rating hiện có
- gán vào `movie.RatingCount`
- nếu không còn rating nào:
  - `movie.AvgRating = null`
- nếu còn rating:
  - lấy trung bình `Score`
  - làm tròn 2 chữ số thập phân

Lưu ý:

- `AvgRating` là `decimal?`
- logic này giữ dữ liệu aggregate nhất quán với bảng `MovieRating`

#### f. `BuildCommentTree`

Mục tiêu:

- chuyển danh sách comment phẳng thành cây comment

Thuật toán:

- tạo `lookup` từ `CommentId -> CommentResponse`
- duyệt toàn bộ comment
- nếu có `ParentId` và parent tồn tại trong lookup:
  - add node hiện tại vào `parent.ChildComments`
- nếu không:
  - add node vào danh sách root
- sắp xếp child nodes theo `CreatedAt`
- trả về danh sách root đã sắp xếp

Lợi ích:

- độ phức tạp tuyến tính theo số comment
- không gọi DB thêm cho từng node con
- dễ kiểm soát thứ tự hiển thị

---

## 6. Controller layer

### `ReviewsController`

File: [Controllers/ReviewsController.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Controllers/ReviewsController.cs)

Controller này là entry point cho API review.

Attributes:

- `[ApiController]`
- `[Authorize]`
- `[Route("api/[controller]")]`

Nghĩa là:

- mọi endpoint trong controller đều yêu cầu user đăng nhập
- user id được lấy từ JWT claims qua `ICurrentUserService`

Endpoints:

- `POST api/reviews/ratings`
- `DELETE api/reviews/ratings/{movieId}`
- `POST api/reviews/comments`
- `GET api/reviews/movies/{movieId}/comments`

Cách controller làm việc:

- nhận request DTO
- lấy current user id
- gọi service
- trả về `ApiResponse<T>`

Tại sao controller không chứa logic:

- giữ controller mỏng
- nghiệp vụ nằm ở service
- dễ test và dễ bảo trì hơn

---

## 7. DI registration

### `ReviewModuleExtensions`

File: [Extensions/ReviewModuleExtensions.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Extensions/ReviewModuleExtensions.cs)

Extension này đăng ký toàn bộ dependency của module review.

Đăng ký:

- `AddHttpContextAccessor()`
- `ICurrentUserService -> CurrentUserService`
- `IReviewService -> ReviewService`

Lý do:

- tuân thủ rule của repo: không nhét business registration vào `Program.cs`
- module review được cấu hình tập trung trong một file riêng

### `ServiceCollectionExtensions`

File: [Extensions/ServiceCollectionExtensions.cs](D:/IT_K22/CCNLTHD/ReviewFilms/ReviewFilms/.worktree/notification/Extensions/ServiceCollectionExtensions.cs)

File này đã được nối thêm `services.AddReviewModule();` trong `AddApiControllers()`.

Ý nghĩa:

- giữ `Program.cs` sạch
- đảm bảo module review được đăng ký cùng lúc với controller setup

---

## 8. Mapping với Entity có sẵn

JOB-005 không sửa entity, chỉ dùng các entity đã có:

- `MovieRating`
- `Comment`
- `CommentVote`
- `Movie`
- `User`

Các trường quan trọng đang được sử dụng:

- `MovieRating.MovieId`, `MovieRating.UserId`, `MovieRating.Score`
- `Comment.ParentId`, `Comment.RootId`, `Comment.Depth`, `Comment.ReplyCount`
- `Movie.AvgRating`, `Movie.RatingCount`, `Movie.CommentCount`

Điểm cần lưu ý:

- vì entity đã có `RootId`, thuật toán tạo reply có thể gắn thread gốc rõ ràng
- vì entity đã có `ReplyCount`, UI có thể hiển thị số reply nhanh hơn

---

## 9. Luồng xử lý end-to-end

### 9.1 User rating phim

- client gọi `POST /api/reviews/ratings`
- controller lấy `userId` từ claims
- service upsert rating
- service tính lại `AvgRating`
- controller trả `ApiResponse<object>`

### 9.2 User xóa rating

- client gọi `DELETE /api/reviews/ratings/{movieId}`
- controller lấy `userId`
- service xóa record rating
- service tính lại aggregate movie
- controller trả response thành công

### 9.3 User tạo comment gốc

- client gọi `POST /api/reviews/comments` với `ParentId = null`
- service tạo comment mới
- `RootId` được set bằng chính `Id` của comment mới
- comment được lưu ở mức root

### 9.4 User reply comment

- client gọi `POST /api/reviews/comments` với `ParentId = comment cha`
- service load parent comment
- service gán `RootId` theo thread gốc
- service tăng `Depth`
- service tăng `ReplyCount`
- comment được lưu đúng vào cây

### 9.5 Lấy danh sách comment

- client gọi `GET /api/reviews/movies/{movieId}/comments`
- service query một lần toàn bộ node comment
- service dựng tree trong memory
- controller trả về danh sách root với `ChildComments`

---

## 10. Xử lý lỗi

Module này cố tình dùng exception thay vì try-catch rải rác.

Các lỗi thường gặp:

- `ArgumentException` khi score hoặc content không hợp lệ
- `KeyNotFoundException` khi movie/comment cha không tồn tại
- `InvalidOperationException` khi comment cha không còn visible
- `UnauthorizedAccessException` khi không lấy được user id từ claims

Các lỗi này sẽ được `GlobalExceptionMiddleware` format thành JSON thống nhất.

---

## 11. Kiểm tra đã thực hiện

Đã chạy:

- `dotnet build`

Kết quả:

- build thành công
- `0 Warning(s)`
- `0 Error(s)`

---

## 12. Ghi chú kỹ thuật

- Module review đang sẵn sàng cho JWT claims, nhưng app host vẫn cần authentication pipeline đúng chuẩn để `[Authorize]` hoạt động đầy đủ.
- Comment tree hiện được dựng từ một flat query, nên không phát sinh N+1.
- Nếu sau này cần pagination cho comment thread lớn, có thể tách thêm endpoint trả root comments theo page và load reply theo batch.

