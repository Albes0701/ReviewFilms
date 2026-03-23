# JOB-002: Ánh xạ Database Schema (EF Core PostgreSQL)

**Ngày thực hiện:** 2026-03-24  
**Phạm vi:** Chỉ làm `Enums`, `Entities`, `ApplicationDbContext`, DI cho PostgreSQL. Không làm seeder, repository hay service.

---

## 1. JOB-002 làm gì

Mục tiêu của JOB-002 là chuyển schema trong `DanhGiaPhim.sql` thành code EF Core để dự án có thể:

- hiểu được toàn bộ bảng, khóa, quan hệ và enum của PostgreSQL
- sinh migration đầu tiên từ code-first
- giữ đúng kiến trúc `AGENT.md`
- chuẩn bị sẵn nền móng để các job sau chỉ cần thêm nghiệp vụ

Nói ngắn gọn: JOB-002 là bước “dịch schema SQL thành model C#”.

Tham chiếu file SQL đầu vào:
- `C:\Users\Nguyen Quy Hung\Downloads\DanhGiaPhim.sql`

---

## 2. Kết quả tổng quan

### Đã tạo

- `9` enum C# trong `/Enums`
- `17` entity POCO trong `/Entities`
- `ApplicationDbContext` trong `/Data`
- đăng ký `DbContext` PostgreSQL trong `/Extensions/ServiceCollectionExtensions.cs`
- cập nhật `Program.cs` để DI DbContext
- cập nhật `ReviewFilms.csproj` để có package EF Core / Npgsql cần thiết

### Đã xác nhận

- `dotnet build` chạy thành công
- không có warning hoặc error sau khi xử lý file exe bị khóa

---

## 3. Cấu hình package đã thêm

Trong [ReviewFilms.csproj](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\ReviewFilms.csproj) tôi đã thêm các package cần cho EF Core + PostgreSQL:

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.Tools`
- `Npgsql`
- `Npgsql.EntityFrameworkCore.PostgreSQL`
- `EFCore.NamingConventions`

Ý nghĩa:

- `Microsoft.EntityFrameworkCore` là lõi EF Core
- `Design` và `Tools` cần cho migration
- `Npgsql` là provider PostgreSQL
- `Npgsql.EntityFrameworkCore.PostgreSQL` là cầu nối EF Core với PostgreSQL
- `EFCore.NamingConventions` giúp áp quy tắc snake_case cho tên database

Tham chiếu:
- [ReviewFilms.csproj](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\ReviewFilms.csproj:1)

---

## 4. Enum mapping

SQL có `CREATE TYPE ... AS ENUM` cho các giá trị sau:

- `user_status`
- `movie_status`
- `credit_type`
- `comment_status`
- `watchlist_status`
- `report_target_type`
- `report_status`
- `notification_type`
- `vote_type`

Tôi đã tạo 9 file enum C# tương ứng trong `/Enums`.

### Cách đặt tên

Trong C# tôi dùng PascalCase:

- `UserStatus`
- `MovieStatus`
- `CreditType`
- `CommentStatus`
- `WatchlistStatus`
- `ReportTargetType`
- `ReportStatus`
- `NotificationType`
- `VoteType`

### Cách giữ đúng label PostgreSQL

Tôi dùng `[PgName("...")]` trên từng member enum để EF/Npgsql map đúng chuỗi trong PostgreSQL, ví dụ:

- `Active` -> `ACTIVE`
- `Published` -> `PUBLISHED`
- `PlanToWatch` -> `PLAN_TO_WATCH`

Tại sao cách này tốt:

- code C# vẫn sạch và đúng convention
- dữ liệu trong PostgreSQL vẫn đúng label gốc
- tránh lỗi mismatch khi tạo migration hoặc đọc dữ liệu

Tham chiếu:
- [UserStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\UserStatus.cs:1)
- [MovieStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\MovieStatus.cs:1)
- [ReportStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\ReportStatus.cs:1)
- [VoteType.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\VoteType.cs:1)

### Ghi chú review

Trong `ApplicationDbContext` tôi vừa dùng:

- `HasPostgresEnum<T>()` để khai báo enum ở model level
- `MapEnum<T>()` trong `UseNpgsql(...)` để provider biết cách serialize/deserialize enum khi kết nối

Đây là cách an toàn để giữ enum hoạt động xuyên suốt từ migration đến runtime.

---

## 5. Entities đã tạo

Tổng cộng có 17 entity, đúng với 17 bảng trong SQL.

### 5.1. Nhóm Identity & Security

- `User`
- `Role`
- `Permission`
- `RolePermission`
- `UserRole`
- `RefreshToken`

### 5.2. Nhóm Content

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

Tham chiếu đại diện:
- [User.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\User.cs:1)
- [Movie.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Movie.cs:1)
- [Comment.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Comment.cs:1)
- [Report.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Report.cs:1)

### 5.3. Quy ước dữ liệu

Tôi áp dụng các quy tắc sau:

- tất cả `uuid` trong SQL được map sang `Guid`
- tên class và property dùng PascalCase
- navigation property dùng `ICollection<T>` cho quan hệ 1-N hoặc N-N
- các thuộc tính optional trong SQL dùng nullable trong C#

Ví dụ:

- `avatar_url` -> `AvatarUrl`
- `created_at` -> `CreatedAt`
- `deleted_at` -> `DeletedAt`
- `movie_id` -> `MovieId`

### 5.4. Ví dụ thực tế

Trong [User.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\User.cs:1):

- `Id` là `Guid`
- `Status` dùng `UserStatus`
- có navigation đến `UserRoles`, `RefreshTokens`, `CreatedMovies`, `Comments`, `Watchlists`, `Reports`, `Notifications`

Trong [Comment.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Comment.cs:1):

- có self-reference qua `ParentId` và `RootId`
- có navigation cha/con để support thread comment
- có `CommentVotes` để phản ánh vote trên comment

Trong [Report.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Report.cs:1):

- `TargetType` dùng enum
- `TargetId` vẫn là `Guid` vì SQL cho phép trỏ vào nhiều loại target
- có 2 quan hệ tới `User`: người report và người review report

---

## 6. Những điểm mapping quan trọng trong Entities

### 6.1. Bảng quan hệ nhiều-nhiều

SQL có các bảng join:

- `role_permission`
- `user_role`
- `movie_genres`

Tôi map chúng thành entity riêng, không dùng implicit many-to-many, vì:

- bảng join có thêm metadata như `created_at`, `assigned_at`
- cần kiểm soát composite key rõ ràng
- tương thích tốt hơn với schema SQL gốc

Ví dụ:

- [RolePermission.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\RolePermission.cs:1)
- [UserRole.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\UserRole.cs:1)
- [MovieGenre.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\MovieGenre.cs:1)

### 6.2. Self-reference

Bảng `comment` có:

- `parent_id`
- `root_id`

Tôi thêm navigation self-reference trong entity `Comment` để EF hiểu được cây comment:

- `ParentComment` / `ChildComments`
- `RootComment` / `ThreadComments`

Đây là chỗ cần review kỹ vì logic thread comment thường dễ bị hiểu nhầm giữa “parent” và “root”.

### 6.3. Tên bảng không theo chuyển đổi tự động

Một số bảng trong SQL không hoàn toàn là chuyển đổi chuẩn từ PascalCase sang snake_case, ví dụ:

- `moviecredits`
- `movie_rating`
- `movie_genres`
- `comment_vote`
- `genres`
- `persons`

Vì vậy tôi vừa dùng `UseSnakeCaseNamingConvention()`, vừa vẫn gọi `ToTable(...)` ở các entity cần thiết để khớp đúng schema gốc.

---

## 7. ApplicationDbContext

File trung tâm là:

- [ApplicationDbContext.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Data\ApplicationDbContext.cs:1)

### 7.1. DbSet

Tôi khai báo `DbSet<T>` cho toàn bộ entity:

- `Users`
- `Roles`
- `Permissions`
- `RolePermissions`
- `UserRoles`
- `RefreshTokens`
- `Movies`
- `Genres`
- `Persons`
- `MovieCredits`
- `MovieGenres`
- `MovieRatings`
- `Comments`
- `CommentVotes`
- `Watchlists`
- `Reports`
- `Notifications`

### 7.2. `OnModelCreating`

Tôi override `OnModelCreating` để cấu hình toàn bộ schema bằng Fluent API.

Lý do:

- không phụ thuộc naming convention ngầm
- kiểm soát được key, index, unique constraint, delete behavior
- dễ so khớp với SQL gốc hơn Data Annotation

### 7.3. Mapping enum

Trong `ConfigurePostgresEnums(...)` tôi khai báo:

- `HasPostgresEnum<UserStatus>()`
- `HasPostgresEnum<MovieStatus>()`
- `HasPostgresEnum<CreditType>()`
- `HasPostgresEnum<CommentStatus>()`
- `HasPostgresEnum<WatchlistStatus>()`
- `HasPostgresEnum<ReportTargetType>()`
- `HasPostgresEnum<ReportStatus>()`
- `HasPostgresEnum<NotificationType>()`
- `HasPostgresEnum<VoteType>()`

### 7.4. PK / FK / unique index

Tôi cấu hình hầu hết bảng theo đúng SQL:

- `HasKey(...)` cho primary key
- `HasIndex(...).IsUnique()` cho unique index
- `HasForeignKey(...)` cho foreign key
- `OnDelete(DeleteBehavior.NoAction)` để tránh cascade ngoài ý muốn

Điểm này rất quan trọng với schema có nhiều quan hệ chéo như:

- `User -> ReportsFiled`
- `User -> ReportsReviewed`
- `Movie -> Comments`
- `Comment -> ParentComment`
- `RefreshToken -> ReplacedByToken`

### 7.5. Join table composite key

Các bảng join được cấu hình composite key:

- `role_permission`: `(RoleId, PermissionId)`
- `user_role`: `(UserId, RoleId)`
- `movie_genres`: `(MovieId, GenreId)`

### 7.6. Cột đặc biệt

Tôi xử lý các cột đặc biệt bằng Fluent API:

- `created_by`
- `assigned_by`
- `reviewed_by`
- `data_json` -> `jsonb`
- `score` trong `movie_rating` có comment `Suggested range: 1..10`

### 7.7. Kiểu ngày giờ

Tôi thêm `ApplyTemporalColumnTypes(...)` để map:

- `DateTime` -> `timestamp without time zone`
- `DateOnly` -> `date`

Mục đích:

- tránh EF/Npgsql tự suy diễn sai kiểu cột
- làm cho migration tạo schema gần với SQL gốc hơn

### 7.8. Review note kỹ thuật

`UseSnakeCaseNamingConvention()` giúp giảm việc phải map tên cột thủ công, nhưng không thay thế hoàn toàn Fluent API.

Chỗ nào SQL cần tên cụ thể thì tôi vẫn explicit map:

- `ToTable("moviecredits")`
- `ToTable("genres")`
- `Property(...).HasColumnName("created_by")`

Đây là cách thực tế nhất để vừa sạch code vừa giữ đúng schema.

---

## 8. DI và Program

### 8.1. Đăng ký DbContext

Trong [ServiceCollectionExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\ServiceCollectionExtensions.cs:1) tôi thêm:

- `AddApplicationDbContext(IConfiguration configuration)`

Chức năng:

- đọc `ConnectionStrings:DefaultConnection`
- đăng ký `ApplicationDbContext`
- cấu hình `UseNpgsql(...)`
- map enum PostgreSQL
- bật `UseSnakeCaseNamingConvention()`

### 8.2. `Program.cs`

Tôi cập nhật [Program.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Program.cs:1) để gọi:

- `builder.Services.AddApplicationDbContext(builder.Configuration);`
- `builder.Services.AddApiControllers();`
- `builder.Services.AddApiSwagger();`

Điều này giữ `Program.cs` gọn và đúng tinh thần `AGENT.md`.

---

## 9. Package cần cài bằng CLI

Nếu muốn tái tạo thủ công, đây là lệnh cài package:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.1

dotnet add package EFCore.NamingConventions --version 10.0.1
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.1
dotnet add package Microsoft.EntityFrameworkCore.Design --version 10.0.1
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 10.0.1
dotnet add package Npgsql --version 10.0.1
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
```

Ghi chú:

- trong implementation hiện tại, các package này đã được thêm vào `.csproj`
- lệnh trên là để bạn học / tái tạo nếu cần

---

## 10. Lệnh tạo migration

Lệnh migration đầu tiên:

```powershell
dotnet ef migrations add InitialCreate
```

Sau đó update database:

```powershell
dotnet ef database update
```

Nếu migration chưa chạy được vì thiếu tool, cài `dotnet-ef` trước.

---

## 11. Kết quả kiểm tra

Tôi đã chạy:

```powershell
dotnet build
```

Kết quả cuối:

- build thành công
- `0 Warning(s)`
- `0 Error(s)`

Trong quá trình build có gặp lỗi file `ReviewFilms.exe` bị khóa bởi process đang chạy. Tôi đã dừng process đó rồi build lại thành công.

---

## 12. Cách bạn nên review code

Nếu bạn muốn học cách đọc JOB-002, nên đọc theo thứ tự này:

1. [ReviewFilms.csproj](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\ReviewFilms.csproj:1)
2. [Program.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Program.cs:1)
3. [Extensions/ServiceCollectionExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\ServiceCollectionExtensions.cs:1)
4. [Data/ApplicationDbContext.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Data\ApplicationDbContext.cs:1)
5. [Entities/User.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\User.cs:1)
6. [Entities/Comment.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Comment.cs:1)
7. [Entities/Report.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Report.cs:1)
8. Các file enum trong `/Enums`

### Khi review, hãy kiểm tra 5 điểm này

- enum C# có khớp enum PostgreSQL không
- mỗi `uuid` đã được đổi sang `Guid` chưa
- các bảng join đã có composite key chưa
- các foreign key có đúng tên và đúng quan hệ không
- `Program.cs` có sạch và chỉ làm wiring không

---

## 13. Những điểm cần nhớ để không làm sai ở job sau

- Không return entity trực tiếp ra API
- Không đưa business logic vào `DbContext`
- Không tạo repository/service trong JOB-002
- Khi thêm job sau, luôn đọc schema SQL trước rồi mới làm migration
- Nếu schema thay đổi, update entity và Fluent API đồng thời

---

## 14. Kết luận

JOB-002 đã chuyển schema SQL sang EF Core model thành công:

- enum đã được map đúng
- entity đã phủ đủ bảng
- quan hệ và key đã được cấu hình bằng Fluent API
- PostgreSQL DbContext đã được đăng ký
- project build sạch

Bạn có thể dùng file này như tài liệu đọc nhanh để review lại code và đối chiếu trực tiếp với SQL gốc.

