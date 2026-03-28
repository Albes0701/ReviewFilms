# JOB-002: Ánh xạ Database Schema (EF Core MySQL)

**Ngày thực hiện:** 2026-03-24  
**Mục tiêu:** Chuyển schema trong `DanhGiaPhim.sql` thành EF Core model và DbContext dùng MySQL, không viết seeder/repository/service ở bước này.

---

## 1. JOB-002 đã làm gì

JOB-002 là bước dựng tầng dữ liệu cho ReviewFilms API:

- tạo enums để biểu diễn các cột trạng thái/loại
- tạo POCO entities cho toàn bộ bảng trong SQL
- cấu hình `ApplicationDbContext` bằng Fluent API
- đăng ký `DbContext` với MySQL trong DI
- sinh migration đầu tiên để sẵn sàng apply schema lên MySQL Server

Nói ngắn gọn: chuyển schema SQL thành model C# có thể migrate được.

---

## 2. Kết quả tổng quan

### Đã tạo

- `9` enum trong `/Enums`
- `17` entity trong `/Entities`
- `ApplicationDbContext` trong `/Data`
- cấu hình DbContext trong `/Extensions/ServiceCollectionExtensions.cs`
- migration MySQL đầu tiên trong `/Migrations`

### Đã xác nhận

- `dotnet build` chạy thành công
- `dotnet-ef migrations add InitialCreate_MySql` chạy thành công

---

## 3. Package đã dùng

Trong [ReviewFilms.csproj](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\ReviewFilms.csproj) hiện có:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `MySql.EntityFrameworkCore`
- `EFCore.NamingConventions`

Ý nghĩa:

- `Microsoft.EntityFrameworkCore` là lõi EF Core
- `Design` và `Tools` cần cho migration
- `MySql.EntityFrameworkCore` là provider MySQL
- `EFCore.NamingConventions` giúp ánh xạ snake_case cho tên bảng/cột

---

## 4. Enums

9 enum đã được tạo để map các giá trị trạng thái/loại trong schema:

- `UserStatus`
- `MovieStatus`
- `CreditType`
- `CommentStatus`
- `WatchlistStatus`
- `ReportTargetType`
- `ReportStatus`
- `NotificationType`
- `VoteType`

### Cách lưu enum

Trong MySQL, enum được lưu dưới dạng chuỗi bằng `HasConversion<string>()`.

Lý do chọn cách này:

- dễ đọc trong database
- dễ debug khi inspect dữ liệu
- tránh phụ thuộc vào thứ tự giá trị enum
- an toàn hơn khi mở rộng enum sau này

---

## 5. Entities

Tổng cộng có 17 entity, tương ứng 17 bảng trong schema.

### Nhóm Identity & Security

- `User`
- `Role`
- `Permission`
- `RolePermission`
- `UserRole`
- `RefreshToken`

### Nhóm Content

- `Movie`
- `Genre`
- `Person`
- `MovieCredit`
- `MovieGenre`
- `MovieRating`
- `Comment`
- `CommentVote`
- `Watchlist`
- `Report`
- `Notification`

### Quy ước áp dụng

- `uuid` trong SQL map sang `Guid`
- class và property dùng `PascalCase`
- navigation property dùng `ICollection<T>` cho quan hệ 1-N hoặc N-N
- thuộc tính nullable trong SQL map sang nullable C#

Ví dụ:

- `movie_id` -> `MovieId`
- `created_at` -> `CreatedAt`
- `deleted_at` -> `DeletedAt`

---

## 6. ApplicationDbContext

File trung tâm là [ApplicationDbContext.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Data\ApplicationDbContext.cs:1).

### DbSet

Đã khai báo `DbSet<T>` cho toàn bộ entity.

### OnModelCreating

Tôi dùng Fluent API để cấu hình:

- primary key
- foreign key
- unique index
- composite key cho bảng join
- self-reference cho comment
- column name đặc biệt
- kiểu dữ liệu đặc biệt

### Enum properties

9 enum được map bằng:

- `HasConversion<string>()`

Áp dụng cho:

- `User.Status`
- `Movie.Status`
- `MovieCredit.CreditType`
- `Comment.Status`
- `Watchlist.Status`
- `Report.TargetType`
- `Report.Status`
- `Notification.Type`
- `CommentVote.VoteType`

### Kiểu dữ liệu đặc biệt

Tôi map các kiểu đặc biệt như sau:

- `DateTime` -> `datetime(6)`
- `DateOnly` -> `date`
- `Notification.DataJson` -> `json`

### Quan hệ quan trọng

- `User` - `Role` qua `UserRole`
- `Role` - `Permission` qua `RolePermission`
- `Movie` - `Genre` qua `MovieGenre`
- `Comment` tự tham chiếu qua `ParentId` và `RootId`
- `RefreshToken` tự tham chiếu qua `ReplacedByTokenId`

### Bảng join dùng composite key

- `role_permission` với `(RoleId, PermissionId)`
- `user_role` với `(UserId, RoleId)`
- `movie_genres` với `(MovieId, GenreId)`

---

## 7. DI và Program

Trong [ServiceCollectionExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\ServiceCollectionExtensions.cs:1):

- đọc `ConnectionStrings:DefaultConnection`
- đăng ký `ApplicationDbContext`
- cấu hình `UseMySQL(connectionString)`
- giữ `UseSnakeCaseNamingConvention()`

Trong [Program.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Program.cs:1):

- chỉ gọi các extension method
- không chứa business logic
- không chứa cấu hình DB trực tiếp

---

## 8. Migration

Migration đầu tiên đã được tạo:

```powershell
dotnet-ef migrations add InitialCreate_MySql --output-dir Migrations
```

Nếu muốn apply lên database:

```powershell
dotnet-ef database update
```

---

## 9. Kết quả kiểm tra

Đã chạy:

```powershell
dotnet build
```

Kết quả:

- build thành công
- `0 Warning(s)`
- `0 Error(s)`

---

## 10. Checklist review nhanh

Khi review JOB-002, hãy soi các điểm sau:

- `ReviewFilms.csproj` đã dùng MySQL provider chưa
- `ServiceCollectionExtensions.cs` đã dùng `UseMySQL(...)` chưa
- `ApplicationDbContext.cs` có còn cấu hình enum hoặc provider cũ không
- 9 enum có được lưu dạng string bằng `HasConversion<string>()` chưa
- `uuid` đã map sang `Guid` chưa
- các bảng join có composite key chưa
- self-reference của `Comment` đã đúng chưa
- `jsonb` đã đổi sang `json` chưa
- `DateTime` đã map sang `datetime(6)` chưa

---

## 11. Kết luận

JOB-002 đã hoàn tất việc map schema sang EF Core model cho MySQL:

- enums đã sẵn sàng
- entities đã phủ đủ bảng
- DbContext đã được cấu hình bằng Fluent API
- DI đã trỏ sang MySQL
- migration đầu tiên đã sinh thành công

File này dùng để đọc nhanh, học cấu trúc và review lại code trước khi đi sang job tiếp theo.
