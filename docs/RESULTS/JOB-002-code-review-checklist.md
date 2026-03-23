# JOB-002 Code Review Checklist

> Mục tiêu: đọc nhanh theo thứ tự ưu tiên để kiểm tra schema mapping EF Core PostgreSQL đã khớp với `DanhGiaPhim.sql`.

## Ưu tiên 1

1. [ReviewFilms.csproj](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\ReviewFilms.csproj)
2. [Program.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Program.cs)
3. [Extensions/ServiceCollectionExtensions.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Extensions\ServiceCollectionExtensions.cs)
4. [Data/ApplicationDbContext.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Data\ApplicationDbContext.cs)

## Ưu tiên 2

5. [Enums/UserStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\UserStatus.cs)
6. [Enums/MovieStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\MovieStatus.cs)
7. [Enums/CreditType.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\CreditType.cs)
8. [Enums/CommentStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\CommentStatus.cs)
9. [Enums/WatchlistStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\WatchlistStatus.cs)
10. [Enums/ReportTargetType.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\ReportTargetType.cs)
11. [Enums/ReportStatus.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\ReportStatus.cs)
12. [Enums/NotificationType.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\NotificationType.cs)
13. [Enums/VoteType.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Enums\VoteType.cs)

## Ưu tiên 3

14. [Entities/User.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\User.cs)
15. [Entities/Movie.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Movie.cs)
16. [Entities/Comment.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Comment.cs)
17. [Entities/Report.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Report.cs)
18. [Entities/RefreshToken.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\RefreshToken.cs)
19. [Entities/MovieCredit.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\MovieCredit.cs)
20. [Entities/MovieRating.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\MovieRating.cs)
21. [Entities/Watchlist.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Watchlist.cs)
22. [Entities/Notification.cs](d:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\Entities\Notification.cs)

## Checklist khi review

- Xác nhận 9 enum C# map đúng label PostgreSQL bằng `[PgName(...)]`
- Xác nhận `HasPostgresEnum()` và `MapEnum()` đều đã có cho các enum cần thiết
- Xác nhận tất cả `uuid` trong SQL đã chuyển sang `Guid`
- Xác nhận các bảng join dùng composite key:
  - `role_permission`
  - `user_role`
  - `movie_genres`
- Xác nhận self-reference của `comment`:
  - `ParentId`
  - `RootId`
- Xác nhận các unique constraint trong SQL đã được map bằng `HasIndex(...).IsUnique()`
- Xác nhận các tên bảng/cột đặc biệt đã map đúng:
  - `moviecredits`
  - `movie_rating`
  - `movie_genres`
  - `comment_vote`
  - `created_by`
  - `assigned_by`
  - `reviewed_by`
- Xác nhận `Program.cs` chỉ làm wiring, không chứa business logic
- Xác nhận build vẫn sạch trước khi tạo migration

