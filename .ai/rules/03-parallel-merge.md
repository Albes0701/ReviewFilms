# LUẬT THI CÔNG WORKTREE SONG SONG (03-PARALLEL-MERGE)

Bạn đang thi công một Module độc lập trong hệ thống Monolithic. Các module khác đang được team khác thi công song song. Để không gây conflict Git khi merge code, PHẢI TUÂN THỦ CÁC LUẬT SAU:

## 1. Vùng Cấm (No-Touch Zones)
- **KHÔNG ĐƯỢC CHỈNH SỬA `Program.cs`.**
- **KHÔNG ĐƯỢC CHỈNH SỬA các file trong `/Entities`** (Cấu trúc DB đã chốt cố định).
- **KHÔNG ĐƯỢC CHỈNH SỬA `ApplicationDbContext.cs`** trừ khi được yêu cầu cực kỳ rõ ràng.

## 2. Quy tắc Đăng ký Service (DI Registration)
Thay vì chèn code vào `Program.cs`, bạn phải tạo một file Extension riêng cho module của mình trong thư mục `/Extensions`:
- Ví dụ Module Film: Tạo `FilmModuleExtensions.cs`.
- Viết hàm `public static IServiceCollection AddFilmModule(this IServiceCollection services)`.
- Đăng ký mọi `AddScoped`, `AddTransient` của bạn vào hàm này.

## 3. Quy tắc Mocking Dữ Liệu Chéo (Cross-Module Mocking)
Nếu module của bạn cần dữ liệu từ module khác (VD: Cần ID User đang đăng nhập, cần gọi hàm gửi thông báo):
- **KHÔNG TỰ VIẾT** logic của module đó.
- Hãy tạo một Interface giả định (VD: `ICurrentUserService` có hàm `Guid GetUserId()`) và tạo một class Mock (VD: `MockCurrentUserService` trả về `Guid.NewGuid()`).
- Đăng ký Mock class đó vào hàm DI Extension của bạn. Khi merge, Tech Lead sẽ thay class Mock bằng class thật.