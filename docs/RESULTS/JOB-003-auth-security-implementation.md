# JOB-003: Auth & Security Implementation Report

**Ngày thực hiện:** 2026-03-30  
**Mục tiêu:** Triển khai xác thực bằng JWT, mật khẩu hash bằng BCrypt, phân quyền RBAC cơ bản, và refresh token flow cho ReviewFilms API.

---

## 1. Mục tiêu của JOB-003

JOB-003 bổ sung lớp bảo mật cho API:

- đăng ký user mới
- đăng nhập bằng username/email + mật khẩu
- phát hành access token JWT
- phát hành refresh token
- gắn role mặc định cho user mới
- hỗ trợ refresh token rotation
- trả response theo `ApiResponse<T>` để frontend Next.js/React dùng ổn định

Phần này chỉ thao tác ở tầng ứng dụng:

- không sửa `Entities`
- không sửa `Program.cs`
- không đổi schema database

---

## 2. Bức tranh tổng thể

Luồng chính hiện tại:

`HTTP Request` -> `AuthController` -> `IAuthService/AuthService` -> `ApplicationDbContext` -> `Entities`

Các thành phần bổ sung:

- DTOs ở `/DTOs/Auth`
- Security helpers ở `/Security`
- Service contract ở `/Interfaces`
- Service logic ở `/Services`
- DI module ở `/Extensions`
- cấu hình JWT ở `/Configurations`
- test refresh token ở `/ReviewFilms.Tests`

Tôi cũng giữ đúng quy ước của repo:

- constructor injection
- async toàn bộ thao tác I/O
- không nhúng logic nghiệp vụ vào controller
- không sửa `Program.cs`

---

## 3. DTOs: dữ liệu vào/ra của Auth

### 3.1 `RegisterRequest`

File: `DTOs/Auth/RegisterRequest.cs`

Chứa thông tin cần để đăng ký:

- `Username`
- `Email`
- `Password`
- `DisplayName` (optional)

Validation dùng Data Annotations:

- `Username` bắt buộc, dài 3..50
- `Email` bắt buộc, đúng format email
- `Password` bắt buộc, dài tối thiểu 8
- `DisplayName` optional, tối đa 100

### 3.2 `LoginRequest`

File: `DTOs/Auth/LoginRequest.cs`

Chứa:

- `UsernameOrEmail`
- `Password`

Frontend có thể gửi username hoặc email trong cùng một field.

### 3.3 `RefreshRequest`

File: `DTOs/Auth/RefreshRequest.cs`

Phần refresh token dùng:

- `AccessToken`
- `RefreshToken`

Lý do thiết kế này:

- bảng `refresh_token` đang lưu `JwtId`
- `JwtId` tương ứng với `jti` của access token
- server dùng `jti` để tìm đúng refresh token row, rồi verify hash của refresh token

### 3.4 `AuthResponse`

File: `DTOs/Auth/AuthResponse.cs`

Response trả về cho register, login và refresh:

- `Token` - access token JWT
- `RefreshToken` - refresh token raw để client lưu
- `TokenType` - mặc định `Bearer`
- `ExpiresAt` - thời điểm access token hết hạn
- `UserId`
- `Username`
- `Email`
- `DisplayName`
- `AvatarUrl`
- `Roles`

Mục tiêu là JSON gọn, dễ dùng cho frontend:

```json
{
  "success": true,
  "message": "Login successful.",
  "data": {
    "token": "...",
    "refreshToken": "...",
    "tokenType": "Bearer",
    "expiresAt": "2026-03-30T08:00:00Z",
    "userId": "...",
    "username": "demo",
    "email": "demo@example.com",
    "displayName": "Demo User",
    "avatarUrl": null,
    "roles": ["USER"]
  }
}
```

---

## 4. Security layer

### 4.1 `PasswordHasher`

File: `Security/PasswordHasher.cs`

Class này bọc BCrypt:

- `Hash(string value)`
- `Verify(string value, string hashedValue)`

Điểm quan trọng:

- password không bao giờ lưu plain-text
- refresh token cũng được hash trước khi lưu DB

Mình dùng cùng một utility vì cả password và refresh token đều là bí mật cần lưu dạng hash.

### 4.2 `JwtTokenGenerator`

File: `Security/JwtTokenGenerator.cs`

Trách nhiệm:

- sinh access token JWT
- sinh refresh token raw ngẫu nhiên

#### Access token

`GenerateAccessToken(...)` tạo JWT với:

- `sub`
- `jti`
- `nameidentifier`
- `name`
- `unique_name`
- `email`
- role claims bằng `ClaimTypes.Role`

JWT dùng:

- secret key từ `JwtOptions.SecretKey`
- issuer từ `JwtOptions.Issuer`
- audience từ `JwtOptions.Audience`

#### Refresh token

`GenerateRefreshToken()` tạo chuỗi random bằng `RandomNumberGenerator` và encode URL-safe.

Chuỗi raw này chỉ trả về cho client, còn DB lưu hash của chuỗi đó.

### 4.3 `JwtOptions`

File: `Configurations/JwtOptions.cs`

Mình tách cấu hình JWT ra một class riêng để:

- bind từ `appsettings.json`
- tránh hard-code secret
- giữ cấu hình tập trung

Các giá trị mặc định:

- `Issuer = "ReviewFilms.Api"`
- `Audience = "ReviewFilms.Frontend"`
- `AccessTokenMinutes = 60`
- `RefreshTokenDays = 30`

---

## 5. Service layer

### 5.1 `IAuthService`

File: `Interfaces/IAuthService.cs`

Contract hiện có:

- `RegisterAsync`
- `LoginAsync`
- `RefreshAsync`

### 5.2 `AuthService`

File: `Services/AuthService.cs`

Đây là phần xử lý nghiệp vụ chính.

#### 5.2.1 Register

Luồng đăng ký:

1. normalize username/email
2. kiểm tra user trùng username hoặc email
3. lấy role mặc định theo `Role.Code = USER`
4. hash password bằng BCrypt
5. tạo `User`
6. tạo `UserRole`
7. phát hành access token + refresh token
8. lưu `RefreshToken` hash vào DB
9. trả `AuthResponse`

Điểm cần nhớ:

- user mới được gán `UserStatus.Active`
- `LastLoginAt` được set ngay khi register vì hệ thống auto-login sau đăng ký
- default role hiện tại là `USER`

Nếu DB chưa có role `USER`, service sẽ throw rõ ràng để báo cần seed dữ liệu.

#### 5.2.2 Login

Luồng đăng nhập:

1. normalize `UsernameOrEmail`
2. tìm user theo username hoặc email
3. chặn nếu user không `Active` hoặc đã bị xóa mềm
4. verify mật khẩu bằng BCrypt
5. load roles qua `UserRoles -> Role`
6. phát hành access token + refresh token mới
7. lưu refresh token hash
8. cập nhật `LastLoginAt`
9. trả `AuthResponse`

#### 5.2.3 Refresh

Refresh flow dùng `RefreshRequest` gồm `AccessToken` và `RefreshToken`.

Luồng:

1. đọc `jti` từ access token
2. tìm bản ghi refresh token theo `JwtId`
3. đảm bảo token chưa revoked, chưa hết hạn, và user vẫn active
4. verify refresh token raw với hash trong DB
5. tạo cặp token mới
6. revoke refresh token cũ
7. gán `ReplacedByTokenId` trỏ sang refresh token mới
8. trả `AuthResponse` mới

Thiết kế này phù hợp với schema hiện có:

- `JwtId` để định danh phiên token
- `RevokedAt` để vô hiệu hóa token cũ
- `ReplacedByTokenId` để trace rotation

### 5.3 Quy tắc normalize

Mình normalize:

- username -> trim + lowercase
- email -> trim + lowercase
- access token jti -> đọc từ JWT claim

Điều này giúp tìm kiếm ổn định hơn và giảm lỗi do case mismatch ở frontend.

---

## 6. Controller layer

### `AuthController`

File: `Controllers/AuthController.cs`

Controller chỉ làm việc định tuyến:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`

Controller:

- nhận request DTO
- gọi service
- trả `Ok(ApiResponse<AuthResponse>.Ok(...))`

Không có business logic trong controller.

Tất cả endpoint auth đều gắn `[AllowAnonymous]` để không bị chặn bởi auth policy khi hệ thống sau này bật authorize rộng hơn.

---

## 7. DI và middleware wiring

### 7.1 `AuthModuleExtensions`

File: `Extensions/AuthModuleExtensions.cs`

Extension này chứa:

- `AddAuthModule(...)`
- `AddJwtAuth(...)`

Nó đăng ký:

- `JwtOptions`
- `PasswordHasher`
- `JwtTokenGenerator`
- `IAuthService -> AuthService`
- authentication scheme JWT bearer
- authorization services
- startup filter để inject `UseAuthentication()`

### 7.2 Vì sao không sửa `Program.cs`

Repo có rule cứng là không sửa `Program.cs`.

Để vẫn chạy được auth, mình:

- gọi `services.AddAuthModule(configuration)` từ `AddApplicationDbContext(...)`
- dùng `IStartupFilter` để tự chèn `app.UseAuthentication()`

Nhờ vậy:

- `Program.cs` không đổi
- auth module vẫn được bật
- vẫn tuân thủ rule song song/worktree của repo

### 7.3 `AddJwtAuth()` cấu hình gì

`AddJwtAuth()` set:

- default authenticate scheme
- default challenge scheme
- JWT bearer validation parameters:
  - validate issuer
  - validate audience
  - validate lifetime
  - validate signing key
  - role claim type
  - name claim type

---

## 8. Cấu hình appsettings

File: `appsettings.json`

Thêm section:

```json
"Jwt": {
  "SecretKey": "CHANGE_THIS_TO_A_LONG_RANDOM_SECRET_AT_LEAST_32_CHARS"
}
```

Các giá trị khác lấy từ default trong `JwtOptions`.

Khi deploy thực tế:

- secret key phải đủ dài và bí mật
- không commit secret thật lên git

---

## 9. `ApiResponse<T>`

File: `DTOs/Common/ApiResponse.cs`

Mình bổ sung thêm `Fail(...)` để đồng nhất cách tạo response wrapper.

Hiện tại success flow auth dùng:

- `ApiResponse<AuthResponse>.Ok(...)`

Frontend sẽ nhận shape ổn định, dễ parse trong Next.js/React.

---

## 10. Test coverage

### 10.1 Test project

Mình thêm `ReviewFilms.Tests`.

### 10.2 Test chính

File:

- `ReviewFilms.Tests/AuthServiceRefreshTests.cs`

Test đang kiểm tra:

- refresh token rotation hoạt động
- token cũ bị revoke
- token mới được cấp
- token cũ được liên kết bằng `ReplacedByTokenId`

### 10.3 Kết quả

Đã chạy:

```powershell
dotnet test ReviewFilms.Tests\ReviewFilms.Tests.csproj
```

Kết quả:

- passed
- `1` test
- `0` failed

---

## 11. Những giả định quan trọng

1. Role mặc định khi register là `USER`.
2. Refresh flow hiện tại cần cả `accessToken` và `refreshToken`.
3. Password và refresh token đều được hash bằng BCrypt trước khi lưu.
4. `AuthResponse` trả về cả token và thông tin user cơ bản để frontend có thể set session ngay.

---

## 12. Tóm tắt các file đã thay đổi

- `[ReviewFilms.csproj](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\ReviewFilms.csproj)`
- `[appsettings.json](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\appsettings.json)`
- `[DTOs/Common/ApiResponse.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\DTOs\Common\ApiResponse.cs)`
- `[Configurations/JwtOptions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Configurations\JwtOptions.cs)`
- `[DTOs/Auth/RegisterRequest.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\DTOs\Auth\RegisterRequest.cs)`
- `[DTOs/Auth/LoginRequest.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\DTOs\Auth\LoginRequest.cs)`
- `[DTOs/Auth/RefreshRequest.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\DTOs\Auth\RefreshRequest.cs)`
- `[DTOs/Auth/AuthResponse.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\DTOs\Auth\AuthResponse.cs)`
- `[Interfaces/IAuthService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Interfaces\IAuthService.cs)`
- `[Security/PasswordHasher.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Security\PasswordHasher.cs)`
- `[Security/JwtTokenGenerator.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Security\JwtTokenGenerator.cs)`
- `[Services/AuthService.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Services\AuthService.cs)`
- `[Controllers/AuthController.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Controllers\AuthController.cs)`
- `[Extensions/AuthModuleExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Extensions\AuthModuleExtensions.cs)`
- `[Extensions/ServiceCollectionExtensions.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\Extensions\ServiceCollectionExtensions.cs)`
- `[ReviewFilms.Tests/AuthServiceRefreshTests.cs](D:\IT_K22\CCNLTHD\ReviewFilms\ReviewFilms\.worktree\auth-security\ReviewFilms.Tests\AuthServiceRefreshTests.cs)`

---

## 13. Kết luận

JOB-003 hiện đã có:

- register
- login
- refresh token rotation
- JWT bearer auth wiring
- BCrypt password hashing
- `ApiResponse<T>` cho success flow
- test xác nhận refresh flow

Nếu cần bước tiếp theo, hướng tự nhiên nhất là:

1. thêm endpoint logout/revoke refresh token
2. bổ sung `[Authorize(Roles = "...")]` cho các module cần RBAC thật sự

