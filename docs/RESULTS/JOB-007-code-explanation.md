# JOB-007 Code Explanation

**Ngày thực hiện:** 2026-04-01  
**Mục tiêu:** Giải thích chi tiết phần code đã triển khai để kết nối 4 module Auth, Film, Review và Notification thành một luồng nghiệp vụ hoàn chỉnh trong cùng monolith.

---

## 1. Tổng quan bài toán

Sau khi các module được merge vào cùng một nhánh, hệ thống tồn tại 3 điểm đứt luồng chính:

- `Notification` vẫn dùng `MockCurrentUserService`, nên cách lấy user hiện tại chưa thống nhất.
- `Film` tạo movie nhưng chưa ghi lại người tạo vào `CreatedByUserId`.
- `Review` tạo reply comment nhưng chưa tạo notification cho tác giả comment gốc.

JOB-007 xử lý ba điểm này mà vẫn giữ đúng các ràng buộc kiến trúc:

- không sửa `/Entities`
- không nhét logic vào `Program.cs`
- tiếp tục dùng layered architecture
- các module giao tiếp với nhau qua interface/service đã có sẵn

---

## 2. Danh sách file chính đã thay đổi

### File production

- `Extensions/ServiceCollectionExtensions.cs`
- `Extensions/ReviewModuleExtensions.cs`
- `Extensions/NotificationModuleExtensions.cs`
- `Services/MovieService.cs`
- `Services/ReviewService.cs`
- `ReviewFilms.csproj`
- `Security/MockCurrentUserService.cs` (đã xóa)

### File test

- `ReviewFilms.Tests/CurrentUserServiceTests.cs`
- `ReviewFilms.Tests/MovieServiceTests.cs`
- `ReviewFilms.Tests/ReviewServiceTests.cs`
- `ReviewFilms.Tests/NotificationResponseTests.cs`
- `ReviewFilms.Tests/Program.cs` (đã xóa để chuyển hẳn sang xUnit)

### File tài liệu nội bộ

- `docs/superpowers/specs/2026-04-01-job-007-cross-module-integration-design.md`
- `docs/superpowers/plans/2026-04-01-job-007-cross-module-integration.md`

---

## 3. Ý tưởng tích hợp tổng thể

Giải pháp triển khai theo hướng tạo **một điểm định danh người dùng dùng chung** cho toàn bộ monolith, rồi để từng module tiêu thụ interface này thay vì tự đăng ký riêng.

Kiến trúc sau JOB-007:

1. `CurrentUserService` là implementation duy nhất của `ICurrentUserService`
2. `MovieService` lấy `UserId` hiện tại từ `ICurrentUserService`
3. `ReviewService` gọi `INotificationService` khi phát sinh reply comment hợp lệ
4. `ServiceCollectionExtensions` là composition root chịu trách nhiệm ghép đủ 4 module

Điểm quan trọng là các module không gọi chéo controller với nhau. Chúng chỉ giao tiếp ở tầng service qua interface. Đây là cách giữ đúng layered architecture và tránh coupling kiểu HTTP nội bộ.

---

## 4. Đồng nhất `ICurrentUserService`

### 4.1 Vấn đề cũ

Trước JOB-007, hệ thống có hai nơi đăng ký `ICurrentUserService`:

- `ReviewModuleExtensions` đăng ký `CurrentUserService`
- `NotificationModuleExtensions` đăng ký `MockCurrentUserService`

Điều này gây ra hai vấn đề:

- DI container có nhiều registration cho cùng một contract
- module Notification có thể chạy với logic user khác hoàn toàn phần Review/Auth

Khi hệ thống đã merge thành monolith, đây không còn là “mock tạm thời” hợp lý nữa, vì các module đang chạy trong cùng tiến trình và phải cùng đọc cùng một JWT.

### 4.2 Cách sửa

File: `Extensions/ServiceCollectionExtensions.cs`

Tôi chuyển registration dùng chung lên composition root:

- `services.AddHttpContextAccessor();`
- `services.AddScoped<ICurrentUserService, CurrentUserService>();`

Sau đó gọi đủ:

- `AddFilmModule(configuration)`
- `AddAuthModule(configuration)`
- `AddReviewModule()`
- `AddNotificationModule()`

Kết quả:

- chỉ còn một implementation duy nhất của `ICurrentUserService`
- mọi module đều dùng chung cách đọc user hiện tại từ JWT claims
- `Program.cs` vẫn sạch vì việc ghép module nằm trong extension

### 4.3 Các file dọn DI

#### `ReviewModuleExtensions.cs`

File này trước đó vừa đăng ký `IReviewService`, vừa đăng ký `HttpContextAccessor` và `ICurrentUserService`.

Sau JOB-007:

- chỉ còn `services.AddScoped<IReviewService, ReviewService>();`

Điều này biến module Review thành consumer thuần của shared user-context thay vì tự sở hữu registration đó.

#### `NotificationModuleExtensions.cs`

File này trước đó đăng ký:

- `AddHttpContextAccessor()`
- `ICurrentUserService -> MockCurrentUserService`
- `INotificationService -> NotificationService`

Sau JOB-007:

- chỉ còn `INotificationService -> NotificationService`

Nghĩa là Notification không còn quyền “tự quyết” current user nữa, mà dùng chung registration trung tâm.

### 4.4 Xóa `MockCurrentUserService`

File `Security/MockCurrentUserService.cs` đã bị xóa hoàn toàn.

Ý nghĩa của việc xóa:

- loại bỏ nguy cơ module Notification chạy sai user context
- loại bỏ duplicate implementation gây khó debug
- phản ánh đúng trạng thái hệ thống: đây không còn là môi trường module độc lập, mà là hệ thống đã tích hợp

---

## 5. Tích hợp Auth vào Film

### 5.1 Vấn đề cũ

Trong `MovieService.CreateMovieAsync`, entity `Movie` được tạo với:

- `CreatedByUserId = null`

Điều này làm mất thông tin người tạo phim, dù JWT user đã có sẵn và API này về nghiệp vụ là do admin thao tác.

### 5.2 Cách sửa constructor

File: `Services/MovieService.cs`

`MovieService` được inject thêm:

- `ICurrentUserService _currentUserService`

Constructor mới nhận thêm dependency này cùng với:

- `ApplicationDbContext`
- `ICloudinaryService`
- `ILogger<MovieService>`

Điểm đáng chú ý:

- đây là constructor injection đúng chuẩn của dự án
- service không tự đọc `HttpContext` trực tiếp
- `MovieService` chỉ phụ thuộc vào abstraction `ICurrentUserService`, không phụ thuộc tầng HTTP

### 5.3 Cách sửa `CreateMovieAsync`

Ngay đầu hàm, service lấy:

- `var currentUserId = _currentUserService.GetCurrentUserId();`

Khi tạo entity `Movie`, trường:

- `CreatedByUserId = currentUserId`

được set thay vì `null`.

### 5.4 Tại sao cách này đúng

Thiết kế này giữ được ba nguyên tắc:

1. **Business logic nằm trong service**  
   Quyết định “phim được tạo bởi ai” là thông tin nghiệp vụ gắn với thao tác tạo movie, nên nó phải nằm trong `MovieService`, không phải controller.

2. **Không rò rỉ HTTP vào service**  
   `MovieService` không đọc claims, không biết gì về `HttpContext`. Nó chỉ dùng interface.

3. **Dễ test**  
   Trong test, có thể stub `ICurrentUserService` để ép service chạy với một `Guid` xác định. Điều này đã được dùng trong `MovieServiceTests`.

---

## 6. Tích hợp Review gọi Notification

### 6.1 Vấn đề cũ

`ReviewService.CreateCommentAsync` đã xử lý được:

- comment gốc
- reply comment
- `ParentId`
- `RootId`
- `Depth`
- `ReplyCount`

Nhưng sau khi lưu reply, service dừng ở đó. Nó chưa phát ra bất kỳ hiệu ứng nghiệp vụ liên module nào.

Vì vậy:

- User B reply comment của User A
- comment được lưu đúng
- nhưng User A không nhận được notification

### 6.2 Bổ sung dependency

File: `Services/ReviewService.cs`

`ReviewService` được inject thêm:

- `INotificationService _notificationService`

Điều này là điểm nối chính giữa module Review và Notification.

Tại sao gọi service trực tiếp là hợp lý ở đây:

- hai module đã cùng nằm trong một monolith
- `INotificationService` là abstraction ổn định cho use case “tạo thông báo”
- yêu cầu JOB-007 là tích hợp business flow ngay, chưa cần event bus hay outbox

### 6.3 Vị trí phát notification

Trong `CreateCommentAsync`, sau đoạn:

- tăng `Movie.CommentCount`
- `_dbContext.Comments.Add(comment)`
- `await _dbContext.SaveChangesAsync(cancellationToken)`

service kiểm tra:

- nếu `request.ParentId.HasValue`

thì gọi:

- `CreateReplyNotificationAsync(comment, userId, cancellationToken)`

Điều này có chủ ý:

- notification chỉ được tạo sau khi reply đã được lưu thành công
- payload cần `comment.Id` của reply mới, nên phải để sau `SaveChangesAsync`

### 6.4 Logic của `CreateReplyNotificationAsync`

Helper này nhận:

- `replyComment`
- `replyAuthorId`
- `CancellationToken`

Luồng xử lý:

1. nếu `replyComment.ParentId` không có giá trị thì return
2. query lại `Comments` để lấy `UserId` của comment cha
3. nếu `parentAuthorId == replyAuthorId` thì return
4. tạo payload JSON:
   - `movieId`
   - `commentId`
5. gọi `_notificationService.CreateNotificationAsync(...)`

### 6.5 Tại sao phải query `parentAuthorId` lại

Trong đoạn xử lý reply trước đó, code đã load `parentComment` để kiểm tra tính hợp lệ. Tuy nhiên helper notification được tách riêng và chỉ nhận `replyComment`.

Thiết kế này có hai lợi ích:

- helper có trách nhiệm rõ ràng, độc lập
- không làm phình chữ ký hàm chính bằng cách truyền quá nhiều biến trung gian

Chi phí query là rất nhỏ vì đây là truy vấn theo khóa chính `Id`, và scope của yêu cầu hiện tại chưa cần tối ưu thêm.

### 6.6 Chặn self-reply

Điều kiện:

- `if (parentAuthorId == replyAuthorId) return;`

đảm bảo:

- user tự trả lời comment của chính mình sẽ không nhận notification

Đây là rule nghiệp vụ bắt buộc, vì nếu không chặn sẽ gây spam thông báo do chính hành động của user tạo ra.

### 6.7 Payload notification

Payload được tạo dưới dạng JSON object:

```json
{
  "movieId": "...",
  "commentId": "..."
}
```

và truyền vào `CreateNotificationAsync` dưới dạng `JsonElement`.

Service Notification sẽ serialize phần này vào `Notification.DataJson`.

Ý nghĩa của hai field:

- `movieId`: frontend biết đang cần điều hướng về phim nào
- `commentId`: frontend biết reply nào vừa được tạo, có thể dùng để focus đúng thread/comment

### 6.8 Notification được tạo như thế nào

Lời gọi hiện tại là:

- `NotificationType.CommentReply`
- title: `"Có người vừa trả lời bình luận của bạn"`
- message: `"Một người dùng vừa phản hồi bình luận của bạn."`

Type dùng enum `CommentReply` thay vì string thô. Điều này giữ đúng model hiện tại của hệ thống và tránh sai chính tả magic string ở tầng DB/API.

---

## 7. Luồng end-to-end sau khi tích hợp

### 7.1 Luồng tạo movie

1. Client gửi request đã kèm access token JWT.
2. ASP.NET authentication pipeline xác thực token.
3. `CurrentUserService` đọc `ClaimTypes.NameIdentifier` hoặc fallback `sub` / `nameid`.
4. `MoviesController` gọi `IMovieService.CreateMovieAsync`.
5. `MovieService` lấy `currentUserId` từ `ICurrentUserService`.
6. `Movie.CreatedByUserId` được gán bằng `currentUserId`.
7. Movie được lưu xuống DB với quan hệ creator đầy đủ.

### 7.2 Luồng reply comment

1. User B gửi request tạo comment với `ParentId`.
2. `ReviewsController` lấy `userId` hiện tại qua `ICurrentUserService`.
3. `ReviewService.CreateCommentAsync` load `parentComment`.
4. Service kiểm tra parent tồn tại và còn `Visible`.
5. Service tính:
   - `ParentId`
   - `RootId`
   - `Depth`
   - `ReplyCount`
6. Reply comment được lưu xuống DB.
7. Service lấy `parentAuthorId`.
8. Nếu `parentAuthorId` khác `replyAuthorId`, service gọi `INotificationService.CreateNotificationAsync(...)`.
9. `NotificationService` tạo một bản ghi `Notification` cho User A.
10. User A có thể đọc notification qua `NotificationsController`.

### 7.3 Luồng lấy notification

Luồng này không thay đổi về controller/service, nhưng giờ nó nhận dữ liệu thật từ Review flow thay vì chỉ dựa vào endpoint test nội bộ của module Notification.

---

## 8. Các thay đổi ở test và lý do

JOB-007 không chỉ sửa production code mà còn thêm test hồi quy để khóa hành vi.

### 8.1 `CurrentUserServiceTests`

Test này xác nhận hai thứ:

- `CurrentUserService` đọc đúng `ClaimTypes.NameIdentifier`
- `AddApplicationDbContext` chỉ đăng ký đúng **một** `ICurrentUserService` và gọi đủ 4 module

Đây là test rất quan trọng vì lỗi DI registration là lỗi khó nhìn bằng mắt khi chỉ đọc code.

### 8.2 `MovieServiceTests`

Test dựng:

- một `ApplicationDbContext` in-memory
- một `StubCurrentUserService`
- một `StubCloudinaryService`

Sau đó gọi `CreateMovieAsync` và assert:

- movie trả về có `CreatedByUserId`
- bản ghi persisted trong DB cũng có `CreatedByUserId`

Test này khóa đúng yêu cầu Auth -> Film.

### 8.3 `ReviewServiceTests`

Có hai ca test:

1. reply comment của người khác -> phải tạo notification
2. self-reply -> không được tạo notification

Ca test đầu còn parse `DataJson` để đảm bảo payload thật sự chứa:

- `movieId`
- `commentId`

Như vậy test không chỉ kiểm tra “có notification”, mà còn kiểm tra nội dung notification đúng nghiệp vụ.

### 8.4 Chuyển `ReviewFilms.Tests/Program.cs` sang xUnit

Project test trước đó còn một harness chạy bằng top-level statements. Tôi chuyển phần test notification DTO sang `NotificationResponseTests.cs` dùng xUnit để:

- `dotnet test` chạy sạch hơn
- toàn bộ test project thống nhất một cơ chế test
- tránh cảnh báo entry point không cần thiết

---

## 9. Các sửa kỹ thuật phụ trợ ngoài business flow

Trong quá trình verification, tôi phát hiện project chưa build sạch hoàn toàn dù chưa liên quan trực tiếp tới JOB-007. Có hai lỗi nền đã được xử lý để việc kiểm chứng tính tích hợp là đáng tin cậy:

### 9.1 `ReviewFilms.csproj` nuốt nhầm thư mục test

Main web project đang compile luôn thư mục `ReviewFilms.Tests`, gây ra:

- lỗi top-level statements
- lỗi thiếu `xUnit` trong web project

Tôi thêm:

- `Compile Remove="ReviewFilms.Tests\\**\\*.cs"`
- `None Remove="ReviewFilms.Tests\\**\\*"`

để tách rạch ròi production project và test project.

### 9.2 Thiếu package runtime cho Auth

`AuthModuleExtensions`, `JwtTokenGenerator` và `PasswordHasher` đang dùng:

- JWT bearer authentication
- BCrypt

nhưng `ReviewFilms.csproj` chưa tham chiếu:

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `BCrypt.Net-Next`

Tôi bổ sung hai package này để `dotnet build` và `dotnet test` phản ánh đúng trạng thái thực của codebase.

Những thay đổi này không đổi business behavior của JOB-007, nhưng là điều kiện cần để xác minh công việc đã tích hợp đúng.

---

## 10. Vì sao giải pháp này phù hợp với layered architecture

JOB-007 có thể được làm theo nhiều cách, nhưng cách đã chọn phù hợp với kiến trúc hiện tại vì:

### 10.1 Không đẩy logic xuống controller

Controller vẫn chỉ:

- nhận request
- lấy current user
- gọi service
- trả response

Mọi quyết định nghiệp vụ như:

- ai là người tạo phim
- khi nào phải phát notification
- payload notification gồm gì

đều nằm ở service layer.

### 10.2 Không sửa entity

Tất cả thay đổi tận dụng field/entity đã có sẵn:

- `Movie.CreatedByUserId`
- `Comment.ParentId`
- `Comment.RootId`
- `Notification.DataJson`
- `Notification.Type`

Vì vậy code tuân thủ đúng ràng buộc “không sửa `/Entities`”.

### 10.3 Giao tiếp qua interface

Review không biết `NotificationService` cụ thể hoạt động ra sao, nó chỉ biết `INotificationService`.

Film không biết JWT claims cụ thể nằm ở đâu, nó chỉ biết `ICurrentUserService`.

Đây là kiểu coupling chấp nhận được trong layered monolith: phụ thuộc vào contract, không phụ thuộc trực tiếp vào HTTP hay implementation chi tiết.

---

## 11. Verification đã chạy

Đã chạy và pass:

```powershell
dotnet test ReviewFilms.Tests/ReviewFilms.Tests.csproj
dotnet build ReviewFilms.csproj
```

Kết quả:

- `ReviewFilms.Tests`: passed `8/8`
- `ReviewFilms.csproj`: build thành công, `0 Warning(s)`, `0 Error(s)`

---

## 12. Kết luận

JOB-007 hoàn thiện luồng liên module theo đúng mục tiêu:

- hệ thống chỉ còn một `ICurrentUserService` dùng chung
- movie mới tạo có `CreatedByUserId` lấy từ user đăng nhập
- reply comment của User B sẽ tạo notification cho User A
- self-reply không tạo notification
- composition root đã gọi đủ 4 module trong monolith

Quan trọng hơn, phần tích hợp này không phá kiến trúc cũ mà tận dụng đúng ranh giới đã có giữa `Interfaces`, `Services`, `Extensions` và `Controllers`. Điều đó giúp các job tiếp theo có thể tiếp tục mở rộng flow mà không cần refactor lại nền tảng của JOB-007.
