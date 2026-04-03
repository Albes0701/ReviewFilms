# JOB-008 Implementation Notes

## 1. Mục tiêu của JOB-008

JOB-008 hoàn thiện phần còn thiếu của module Auth sau khi hệ thống đã có đăng ký, đăng nhập và refresh token. Phạm vi xử lý gồm 3 nhóm chức năng:

1. `POST /api/auth/logout`: thu hồi refresh token hiện tại.
2. `GET /api/auth/me`: lấy hồ sơ của người dùng đang đăng nhập.
3. `PUT /api/auth/me`: cập nhật hồ sơ cá nhân và upload avatar.

Ràng buộc quan trọng của job này là:

- Tuân thủ layered architecture hiện có.
- Không trả Entity trực tiếp ra ngoài API.
- Không sửa file trong thư mục `Entities`.
- Mọi response phải đi theo `ApiResponse<T>`.

## 2. Các file được bổ sung và cập nhật

### File mới

- `DTOs/Auth/LogoutRequest.cs`
- `DTOs/Auth/UpdateUserProfileRequest.cs`
- `DTOs/Auth/UserProfileDto.cs`
- `ReviewFilms.Tests/AuthControllerTests.cs`
- `ReviewFilms.Tests/AuthServiceProfileTests.cs`

### File cập nhật

- `Controllers/AuthController.cs`
- `Interfaces/IAuthService.cs`
- `Services/AuthService.cs`
- `ReviewFilms.Tests/AuthServiceRefreshTests.cs`

## 3. Thiết kế theo Layer

### Controller Layer

`AuthController` chỉ làm 3 việc:

1. Nhận input từ HTTP request.
2. Lấy `userId` từ `ICurrentUserService` với các endpoint cần xác thực.
3. Gọi `IAuthService` và bọc kết quả vào `ApiResponse<T>`.

Controller không chứa business logic xử lý token, query dữ liệu hay upload file. Điều này giữ đúng vai trò của `/Controllers` theo `AGENT.md` và `.ai/rules/01-architecture.md`.

### Service Layer

`AuthService` chịu trách nhiệm toàn bộ logic nghiệp vụ mới:

- tìm và revoke refresh token
- query user kèm roles
- validate dữ liệu profile
- upload avatar thông qua `ICloudinaryService`
- map `User` sang `UserProfileDto`

Việc giữ toàn bộ logic ở service giúp controller mỏng, dễ test và đồng nhất với các module khác trong dự án.

### DTO Layer

DTO mới được thêm để tránh lộ trực tiếp cấu trúc Entity:

- `LogoutRequest`: nhận refresh token khi logout
- `UpdateUserProfileRequest`: nhận `displayName`, `bio`, `avatarFile` dạng `multipart/form-data`
- `UserProfileDto`: dữ liệu trả về cho `/me`

## 4. Chi tiết từng phần

## 4.1. `LogoutRequest`

File: `DTOs/Auth/LogoutRequest.cs`

Mục đích:

- cho phép API logout nhận `refreshToken` từ body
- dùng `[StringLength(500)]` để chặn input quá dài ở tầng model binding

Thiết kế dùng `string?` thay vì `string` vì API còn hỗ trợ fallback từ cookie. Nếu body không có token, controller sẽ đọc từ cookie tên `refreshToken`.

## 4.2. `UpdateUserProfileRequest`

File: `DTOs/Auth/UpdateUserProfileRequest.cs`

DTO này dành cho endpoint `PUT /api/auth/me`.

Các field:

- `DisplayName`: tên hiển thị mới
- `Bio`: mô tả cá nhân
- `AvatarFile`: ảnh đại diện, kiểu `IFormFile`

Điểm quan trọng:

- request này được bind bằng `[FromForm]`
- avatar không đi trong JSON mà đi trong `multipart/form-data`
- `DisplayName` dùng `[StringLength(100)]` để bám giới hạn cột `user.display_name`

## 4.3. `UserProfileDto`

File: `DTOs/Auth/UserProfileDto.cs`

DTO này là contract trả về cho cả `GET /api/auth/me` và `PUT /api/auth/me`.

Các nhóm dữ liệu trả về:

- định danh: `UserId`, `Username`, `Email`
- thông tin hiển thị: `DisplayName`, `AvatarUrl`, `Bio`
- trạng thái tài khoản: `Status`, `EmailConfirmed`, `LastLoginAt`, `CreatedAt`
- phân quyền: `Roles`

Điểm đáng chú ý:

- `Roles` được trả dạng `string[]`, phù hợp với cách module auth hiện tại đang dùng ở `AuthResponse`
- DTO này chỉ chứa dữ liệu cần cho client, không lộ navigation property của Entity

## 4.4. Mở rộng `IAuthService`

File: `Interfaces/IAuthService.cs`

Interface được bổ sung 3 hàm:

```csharp
Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);
Task<UserProfileDto> GetCurrentUserProfileAsync(Guid userId, CancellationToken cancellationToken = default);
Task<UserProfileDto> UpdateCurrentUserProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
```

Ý nghĩa:

- service contract giờ bao phủ cả session management và profile management
- controller chỉ làm việc với abstraction `IAuthService`
- dễ unit test và giữ đúng pattern DI hiện có

## 4.5. Cập nhật constructor của `AuthService`

File: `Services/AuthService.cs`

`AuthService` trước đây chỉ cần:

- `ApplicationDbContext`
- `JwtTokenGenerator`
- `PasswordHasher`
- `IOptions<JwtOptions>`

Sau JOB-008, service nhận thêm:

- `ICloudinaryService`

Lý do:

- phần update profile cần upload avatar
- tận dụng lại đúng service đã có của module Film
- không copy logic upload sang Auth

## 4.6. Giải thích `LogoutAsync`

Vị trí: `Services/AuthService.cs`

Luồng xử lý:

1. Nếu `refreshToken` rỗng hoặc null thì return luôn.
2. Query tất cả refresh token chưa bị revoke:
   - `Where(token => token.RevokedAt == null)`
3. Duyệt danh sách và dùng `PasswordHasher.Verify` để tìm token khớp với token raw nhận từ client.
4. Nếu không tìm thấy thì return.
5. Nếu tìm thấy:
   - gán `matchedRefreshToken.RevokedAt = DateTime.UtcNow`
   - `SaveChangesAsync`

Vì sao không query trực tiếp bằng token raw:

- DB đang lưu `TokenHash`, không lưu refresh token dạng plain text
- muốn khớp token phải verify theo cơ chế hash hiện có

Lưu ý:

- hàm này thiết kế idempotent: token không tồn tại hoặc đã invalid cũng không ném lỗi
- hành vi này phù hợp với logout vì mục tiêu là kết thúc session hiện có, không cần tiết lộ token đó có hợp lệ hay không

## 4.7. Giải thích `GetCurrentUserProfileAsync`

Vị trí: `Services/AuthService.cs`

Luồng xử lý:

1. Gọi helper `GetUserWithRolesAsync(userId, cancellationToken)`
2. Nếu không tìm thấy user hoặc user đã bị soft delete thì ném `KeyNotFoundException`
3. Nếu có user, map sang `UserProfileDto` bằng `MapToUserProfileDto`

Helper `GetUserWithRolesAsync` dùng:

```csharp
.Include(currentUser => currentUser.UserRoles)
    .ThenInclude(userRole => userRole.Role)
```

Mục đích:

- lấy cả thông tin role trong cùng truy vấn
- tránh phải query bổ sung sau đó
- đảm bảo DTO trả về có đủ `Roles`

## 4.8. Giải thích `UpdateCurrentUserProfileAsync`

Vị trí: `Services/AuthService.cs`

Đây là phần có nhiều logic nhất của JOB-008.

Luồng xử lý:

1. Load user hiện tại bằng `GetUserWithRolesAsync`
2. Nếu `DisplayName` có truyền lên:
   - nếu chỉ có khoảng trắng thì ném `ValidationException`
   - nếu hợp lệ thì `Trim()` trước khi lưu
3. Nếu `Bio` có truyền lên:
   - nếu rỗng hoặc trắng thì đưa về `null`
   - nếu có nội dung thì `Trim()` rồi lưu
4. Nếu `AvatarFile` có dữ liệu:
   - gọi `_cloudinaryService.UploadImageAsync(request.AvatarFile, "users/avatars", cancellationToken)`
   - nếu upload thành công và có URL thì cập nhật `AvatarUrl`
5. Cập nhật `UpdatedAt = DateTime.UtcNow`
6. `SaveChangesAsync`
7. Map lại sang `UserProfileDto` để trả về

Các quyết định kỹ thuật:

- Không bắt buộc upload avatar mỗi lần update profile
- Không xóa avatar cũ nếu request không gửi file
- `Bio` rỗng được normalize về `null` để dữ liệu trong DB gọn hơn
- `DisplayName` không cho phép cập nhật thành chuỗi trắng để tránh dữ liệu không hợp lệ

## 4.9. Helper `MapToUserProfileDto`

Vị trí: `Services/AuthService.cs`

Hàm này gom toàn bộ logic map từ `User` sang `UserProfileDto`.

Phần roles được xử lý như sau:

- lấy `Role.Code`
- loại bỏ giá trị null hoặc rỗng
- `Distinct(StringComparer.OrdinalIgnoreCase)` để tránh trùng
- `OrderBy(..., StringComparer.OrdinalIgnoreCase)` để output ổn định

Việc sort role giúp dữ liệu trả về nhất quán hơn giữa các lần gọi API và dễ assertion hơn trong test.

## 4.10. Giải thích thay đổi trong `AuthController`

File: `Controllers/AuthController.cs`

### Điều chỉnh quyền truy cập

Ban đầu class có `[AllowAnonymous]` ở cấp controller. Với JOB-008 điều này không còn phù hợp vì `/me` phải yêu cầu đăng nhập.

Giải pháp:

- bỏ `[AllowAnonymous]` ở cấp class
- gắn `[AllowAnonymous]` riêng cho:
  - `register`
  - `login`
  - `refresh`
- gắn `[Authorize]` riêng cho:
  - `GET /api/auth/me`
  - `PUT /api/auth/me`

Thiết kế này tránh việc `/me` vô tình bị mở public.

### Endpoint `POST /api/auth/logout`

Controller xử lý token đầu vào theo thứ tự:

1. ưu tiên `request.RefreshToken` nếu body có giá trị
2. nếu body rỗng thì đọc `Request.Cookies["refreshToken"]`

Sau đó controller gọi:

```csharp
await _authService.LogoutAsync(refreshToken, cancellationToken);
```

Response:

```csharp
ApiResponse<object>.Ok("Logout successful.")
```

### Endpoint `GET /api/auth/me`

Luồng controller:

1. gọi `_currentUserService.GetCurrentUserId()`
2. truyền `userId` vào `_authService.GetCurrentUserProfileAsync`
3. trả về `ApiResponse<UserProfileDto>`

`ICurrentUserService` được dùng đúng mục đích của nó: tách logic đọc claim khỏi controller.

### Endpoint `PUT /api/auth/me`

Luồng controller:

1. bind request bằng `[FromForm]`
2. lấy `userId` từ `ICurrentUserService`
3. gọi `_authService.UpdateCurrentUserProfileAsync`
4. trả `ApiResponse<UserProfileDto>`

Thêm `[Consumes("multipart/form-data")]` để mô tả rõ contract của endpoint.

## 5. Luồng dữ liệu end-to-end

## 5.1. Logout

Client gửi:

- body có `refreshToken`, hoặc
- cookie `refreshToken`

Sau đó:

1. `AuthController.Logout`
2. `AuthService.LogoutAsync`
3. tìm token đã hash trong bảng `refresh_token`
4. set `revoked_at`
5. trả `ApiResponse<object>`

## 5.2. Get Me

1. JWT được xác thực bởi middleware auth đã có sẵn
2. `AuthController.Me` lấy `userId` từ claims qua `ICurrentUserService`
3. `AuthService.GetCurrentUserProfileAsync` load user + roles
4. map sang `UserProfileDto`
5. trả `ApiResponse<UserProfileDto>`

## 5.3. Update Me

1. request `multipart/form-data` đi vào `AuthController.UpdateMe`
2. lấy `userId` hiện tại
3. `AuthService.UpdateCurrentUserProfileAsync` xử lý:
   - validate dữ liệu
   - upload avatar nếu có
   - cập nhật DB
4. trả profile mới nhất cho client

## 6. Xử lý lỗi và bảo mật

Một số lỗi được tận dụng từ global exception middleware hiện có:

- `ValidationException`: dữ liệu profile không hợp lệ
- `UnauthorizedAccessException`: người dùng chưa đăng nhập
- `KeyNotFoundException`: không tìm thấy user

Các điểm bảo mật đáng chú ý:

- không trả Entity trực tiếp
- `/me` được bảo vệ bằng `[Authorize]`
- logout không làm lộ thông tin token có hợp lệ hay không
- refresh token vẫn được lưu dưới dạng hash
- avatar upload đi qua service dùng chung thay vì thao tác file thủ công

## 7. Test đã thêm

### `AuthServiceProfileTests`

Các case chính:

- `LogoutAsync_revokes_matching_refresh_token`
  - đảm bảo logout set `RevokedAt`
- `GetCurrentUserProfileAsync_returns_profile_with_roles`
  - đảm bảo profile trả đúng user và role
- `UpdateCurrentUserProfileAsync_updates_display_name_bio_and_avatar`
  - đảm bảo update profile lưu đúng dữ liệu và gọi đúng folder upload `users/avatars`

### `AuthControllerTests`

Các case chính:

- `Logout_uses_refresh_token_from_cookie_when_request_body_is_empty`
  - đảm bảo controller fallback từ cookie
- `Me_endpoints_require_authorization`
  - đảm bảo `Me` và `UpdateMe` có `[Authorize]`

### Test cũ được cập nhật

`AuthServiceRefreshTests` được thêm stub `ICloudinaryService` vì constructor của `AuthService` đã có dependency mới.

## 8. Kết quả sau triển khai

JOB-008 hiện cung cấp đầy đủ:

- quản lý logout bằng revoke refresh token
- API lấy hồ sơ người dùng đang đăng nhập
- API cập nhật hồ sơ cá nhân, bao gồm upload avatar
- contract DTO rõ ràng cho phần Auth Profile
- test cho các hành vi quan trọng

Lệnh verify đã chạy:

```powershell
dotnet test ReviewFilms.Tests/ReviewFilms.Tests.csproj
```

Kết quả tại thời điểm hoàn tất: toàn bộ test pass.
