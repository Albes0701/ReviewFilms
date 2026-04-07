# JOB-012 Permission Authorization Code Workflow And Debug Report

## 1. Mục đích tài liệu này

Tài liệu này không lặp lại changelog theo kiểu "đã sửa file nào".

Mục tiêu chính là giúp bạn:

- đọc nhanh luồng phân quyền mới của `JOB-012`
- biết class nào chịu trách nhiệm ở từng bước
- biết dữ liệu `Role` và `Permission` đi từ database vào JWT như thế nào
- biết request đi qua các lớp auth/authorization ra sao trước khi vào `MoviesController`
- biết nên đặt breakpoint ở đâu khi bị lỗi `401`, `403`, thiếu claim, hoặc sai permission
- biết các test nào đang chứng minh logic này

Tài liệu này phù hợp khi bạn cần:

- kiểm tra vì sao user có role nhưng vẫn bị chặn endpoint `Movies`
- kiểm tra vì sao token không chứa `permissions`
- kiểm tra vì sao refresh token xong quyền chưa đổi như kỳ vọng
- debug endpoint `POST /api/movies/sync-genres`
- debug endpoint `POST /api/movies/import/*`
- debug endpoint `POST /api/movies`
- debug endpoint `PUT /api/movies/{id}`

## 2. Bản đồ class và trách nhiệm

## 2.1. `AuthService`

File: `Services/AuthService.cs`

Vai trò:

- load `User` cùng `Roles` và `Permissions`
- gom role code và permission code
- gọi `IssueTokensAsync(...)`
- map `UserProfileDto`
- đảm bảo `AuthResponse` trả ra đủ `Roles` và `Permissions`

Không làm:

- không tự validate policy authorization của endpoint
- không quyết định request có được đi vào controller hay không

Nói ngắn gọn:

- đây là nơi "đổ dữ liệu quyền" từ DB vào DTO và JWT

## 2.2. `JwtTokenGenerator`

File: `Security/JwtTokenGenerator.cs`

Vai trò:

- sinh access token JWT
- giữ backward compatibility bằng role claims
- thêm custom permission claims với tên `"permissions"`

Nói ngắn gọn:

- đây là nơi biến dữ liệu quyền trong memory thành claims thật trong JWT

## 2.3. `HasPermissionAttribute`

File: `Security/HasPermissionAttribute.cs`

Vai trò:

- cung cấp cú pháp `[HasPermission("movies:import")]`
- tự động đổi permission thành policy name dạng `Permission:movies:import`

Nói ngắn gọn:

- đây là lớp bridge giữa controller annotation và authorization policy

## 2.4. `PermissionAuthorizationPolicyProvider`

File: `Security/PermissionAuthorizationPolicyProvider.cs`

Vai trò:

- nhận policy name từ `[Authorize(Policy = "...")]`
- nếu policy bắt đầu bằng `Permission:` thì tự build `AuthorizationPolicy`
- gắn `PermissionRequirement`
- giữ fallback cho các policy mặc định khác

Nói ngắn gọn:

- đây là nơi framework hỏi: "policy `Permission:movies:update` nghĩa là gì?"

## 2.5. `PermissionRequirement`

File: `Security/PermissionRequirement.cs`

Vai trò:

- chỉ giữ đúng một thông tin: permission đang yêu cầu là gì

Ví dụ:

- `movies:create`
- `movies:update`
- `genres:sync`

## 2.6. `PermissionAuthorizationHandler`

File: `Security/PermissionAuthorizationHandler.cs`

Vai trò:

- đọc tất cả claim `"permissions"` của user từ `ClaimsPrincipal`
- so sánh với permission đang được yêu cầu
- nếu match thì `context.Succeed(requirement)`
- nếu không match thì để framework trả về deny

Nói ngắn gọn:

- đây là nơi thực sự quyết định user có qua được permission check hay không

## 2.7. `AuthModuleExtensions`

File: `Extensions/AuthModuleExtensions.cs`

Vai trò:

- đăng ký JWT authentication
- đăng ký `PermissionAuthorizationHandler`
- đăng ký `PermissionAuthorizationPolicyProvider`

Nói ngắn gọn:

- đây là nơi wiring DI cho toàn bộ permission authorization

## 2.8. `MoviesController`

File: `Controllers/MoviesController.cs`

Vai trò:

- khai báo endpoint nào cần permission nào
- không chứa logic kiểm tra quyền bằng tay
- chỉ nhận request, đi qua attribute authorization, rồi gọi service

## 3. Dữ liệu quyền đi từ DB đến JWT như thế nào

Nguồn dữ liệu quyền hiện tại trong DB:

`User -> UserRoles -> Role -> RolePermissions -> Permission`

Luồng lấy dữ liệu trong code:

1. `AuthService` query `User`
2. `Include(user => user.UserRoles)`
3. `ThenInclude(userRole => userRole.Role)`
4. `ThenInclude(role => role.RolePermissions)`
5. `ThenInclude(rolePermission => rolePermission.Permission)`
6. từ đây code lấy:
   - `role.Code`
   - `permission.Code`
7. gọi `Distinct(...)`
8. sort bằng `OrderBy(...)`
9. đưa vào:
   - `AuthResponse.Roles`
   - `AuthResponse.Permissions`
   - `UserProfileDto.Roles`
   - `UserProfileDto.Permissions`
   - claims trong JWT

Điểm quan trọng:

- hệ thống hiện tại là hybrid
- role claims vẫn giữ nguyên để các endpoint cũ dùng `[Authorize]` hoặc UI frontend vẫn hoạt động
- permission claims được thêm song song cho các endpoint mới dùng `[HasPermission(...)]`

## 4. Workflow chi tiết khi login

Entry point:

- `AuthController.Login`
- gọi `IAuthService.LoginAsync(...)`

Luồng:

1. `AuthService.LoginAsync(...)` normalize `UsernameOrEmail`
2. query `_dbContext.Users`
3. load sâu toàn bộ role và permission qua `Include/ThenInclude`
4. verify password bằng `_passwordHasher.Verify(...)`
5. cập nhật `LastLoginAt`, `UpdatedAt`
6. gọi `GetDistinctRoles(user)`
7. gọi `GetDistinctPermissions(user)`
8. gọi `IssueTokensAsync(user, roles, permissions, ...)`
9. `IssueTokensAsync(...)` gọi `_jwtTokenGenerator.GenerateAccessToken(...)`
10. `JwtTokenGenerator` thêm:
    - claim identity cơ bản
    - claim role bằng `ClaimTypes.Role`
    - claim permission bằng `"permissions"`
11. `IssueTokensAsync(...)` tạo refresh token mới
12. lưu hash refresh token xuống DB
13. trả `AuthResponse` về controller

Kết quả:

- response chứa cả `Roles` và `Permissions`
- token chứa cả role claims và permission claims

## 5. Workflow chi tiết khi refresh token

Entry point:

- `AuthController.Refresh`
- gọi `IAuthService.RefreshAsync(...)`

Luồng:

1. `RefreshAsync(...)` đọc `jti` từ access token cũ qua `ExtractJwtId(...)`
2. query bảng `RefreshTokens`
3. `Include(token => token.User)`
4. load sâu tiếp toàn bộ role và permission của user
5. verify raw refresh token với `TokenHash`
6. check `User.Status`
7. gọi lại:
   - `GetDistinctRoles(user)`
   - `GetDistinctPermissions(user)`
8. gọi `IssueTokensAsync(...)`
9. issue access token mới với permissions mới nhất đang có trong DB
10. revoke refresh token cũ
11. trả `AuthResponse` mới

Điểm rất quan trọng để debug:

- permission trong access token không tự đổi nếu DB đổi quyền sau khi user đã login
- token chỉ được cập nhật khi user login lại hoặc refresh token thành công

## 6. Workflow chi tiết của `GET /api/auth/me`

Entry point:

- `AuthController.Me`
- endpoint này chỉ cần `[Authorize]`, không dùng `[HasPermission(...)]`

Luồng:

1. request qua JWT authentication
2. `CurrentUserService` lấy `UserId` từ claim
3. `AuthService.GetCurrentUserProfileAsync(...)`
4. `GetUserWithRolesAsync(...)` load sâu role + permission
5. `MapToUserProfileDto(...)`
6. trả `UserProfileDto` gồm:
   - profile cơ bản
   - `Roles`
   - `Permissions`

Điểm dùng để debug frontend:

- nếu frontend render menu theo permission thì xem response `/api/auth/me` trước
- nếu ở đây đã thiếu permission thì lỗi nằm ở service query hoặc mapping
- nếu ở đây đủ permission nhưng endpoint vẫn `403` thì lỗi nằm ở token hoặc authorization handler

## 7. Workflow request vào endpoint `Movies` có `[HasPermission(...)]`

Ví dụ:

- `POST /api/movies/import/bulk`
- attribute là `[HasPermission("movies:import")]`

Luồng runtime:

1. client gửi `Authorization: Bearer <jwt>`
2. `UseAuthentication()` chạy trước nhờ `JwtAuthenticationStartupFilter`
3. JWT bearer middleware validate:
   - signature
   - issuer
   - audience
   - lifetime
4. nếu token hợp lệ, `HttpContext.User` được tạo
5. request gặp `[HasPermission("movies:import")]`
6. `HasPermissionAttribute` đã set `Policy = "Permission:movies:import"`
7. framework gọi `PermissionAuthorizationPolicyProvider.GetPolicyAsync(...)`
8. provider thấy prefix `Permission:`
9. provider build `AuthorizationPolicy`
10. policy chứa `PermissionRequirement("movies:import")`
11. framework gọi `PermissionAuthorizationHandler`
12. handler đọc tất cả claim `"permissions"` trong user principal
13. nếu có `"movies:import"` thì `context.Succeed(...)`
14. request đi tiếp vào `MoviesController.ImportBulk(...)`
15. nếu không có claim này thì request bị chặn trước khi vào action

Kết quả HTTP:

- token sai hoặc hết hạn: thường ra `401 Unauthorized`
- token hợp lệ nhưng thiếu permission: ra `403 Forbidden`

## 8. Mapping permission hiện tại ở `MoviesController`

Các endpoint đã chuyển sang permission-based authorization:

- `POST /api/movies` -> `movies:create`
- `PUT /api/movies/{id}` -> `movies:update`
- `POST /api/movies/sync-genres` -> `genres:sync`
- `POST /api/movies/import/single/{tmdbId}` -> `movies:import`
- `POST /api/movies/import/bulk` -> `movies:import`

Điểm cần lưu ý:

- hiện codebase chưa có `DELETE /api/movies/{id}`
- vì vậy permission `movies:delete` mới chỉ được hỗ trợ ở tầng policy/handler/test, chưa có endpoint thật để gắn attribute

## 9. Breakpoint nên đặt khi debug

## 9.1. Khi nghi ngờ DB đã có permission nhưng token không có

Đặt breakpoint ở:

- `AuthService.LoginAsync(...)`
- `AuthService.RefreshAsync(...)`
- `AuthService.GetDistinctPermissions(...)`
- `AuthService.IssueTokensAsync(...)`
- `JwtTokenGenerator.GenerateAccessToken(...)`

Bạn cần xem:

- `user.UserRoles`
- `role.RolePermissions`
- `rolePermission.Permission.Code`
- biến `permissions`
- danh sách `claims`

## 9.2. Khi token có permission nhưng endpoint vẫn bị `403`

Đặt breakpoint ở:

- `HasPermissionAttribute` constructor
- `PermissionAuthorizationPolicyProvider.GetPolicyAsync(...)`
- `PermissionAuthorizationHandler.HandleRequirementAsync(...)`

Bạn cần xem:

- `Policy` có đúng dạng `Permission:movies:import` không
- `requirement.Permission` là gì
- `context.User.Claims`
- có claim type `"permissions"` hay không
- claim value có đúng chính tả không

## 9.3. Khi endpoint không vào được controller

Đặt breakpoint ở:

- `PermissionAuthorizationHandler.HandleRequirementAsync(...)`
- đầu action trong `MoviesController`

Nếu breakpoint ở controller không hit nhưng handler hit:

- nghĩa là authorization đang chặn request

Nếu handler cũng không hit:

- kiểm tra routing
- kiểm tra attribute có gắn đúng chưa
- kiểm tra authentication đã chạy hay chưa

## 10. Các triệu chứng lỗi thường gặp và cách suy luận

## 10.1. User có role `ADMIN` nhưng vẫn bị chặn endpoint `Movies`

Nguyên nhân thường gặp:

- endpoint mới không còn check role nữa
- endpoint đang check permission cụ thể
- role `ADMIN` trong DB chưa được gắn đúng `RolePermission`

Cách debug:

1. xem bảng `role_permission`
2. xem `permission.code` thực tế
3. login lại hoặc refresh token
4. decode JWT để xem claim `"permissions"`

## 10.2. DB đã thêm permission nhưng user vẫn bị `403`

Nguyên nhân thường gặp:

- user đang dùng access token cũ
- refresh token chưa chạy
- user login trước khi DB đổi quyền

Suy luận:

- handler chỉ đọc claims trong token
- handler không query DB trực tiếp
- nên nếu token cũ thì authorization vẫn dùng dữ liệu cũ

## 10.3. Token có role claims nhưng không có permission claims

Nguyên nhân thường gặp:

- flow issue token chưa nhận `permissions`
- `JwtTokenGenerator.GenerateAccessToken(...)` chưa được gọi đúng tham số
- seed/test data chưa có `RolePermission`

Đặt breakpoint:

- `IssueTokensAsync(...)`
- `GenerateAccessToken(...)`

## 10.4. `/api/auth/me` trả có permission nhưng endpoint vẫn `403`

Khả năng:

- bạn đang gọi endpoint bằng access token cũ, nhưng `/me` đang lấy dữ liệu mới từ DB
- hoặc token dùng cho endpoint không phải token vừa login/refresh

Điểm cần nhớ:

- `/me` query DB trực tiếp
- handler không query DB, nó chỉ đọc token

## 10.5. Endpoint trả `401` thay vì `403`

Điều này thường không phải lỗi permission.

Nó thường là lỗi authentication:

- token hết hạn
- token sai signature
- issuer sai
- audience sai
- header `Authorization` không đúng format

## 11. Walkthrough debug thực tế cho một endpoint bị chặn quyền

Ví dụ cần debug:

- user gọi `POST /api/movies/import/bulk`
- hệ thống trả `403 Forbidden`

Luồng debug đề xuất:

1. xác nhận controller đang gắn `[HasPermission("movies:import")]`
2. decode access token hiện tại
3. kiểm tra token có claim `"permissions": "movies:import"` hay không
4. nếu không có:
   - quay lại debug `AuthService.RefreshAsync(...)` hoặc `LoginAsync(...)`
   - xem user có thật sự mang permission này từ DB không
5. nếu token có claim này:
   - đặt breakpoint ở `PermissionAuthorizationHandler`
   - xem `context.User.FindAll("permissions")`
   - xem có typo hoặc khoảng trắng lạ không
6. nếu handler match nhưng vẫn fail:
   - kiểm tra policy provider có build đúng requirement không
7. nếu handler không match:
   - so sánh chính xác `movies:import` với dữ liệu claim thật

Kết luận debug thường gặp nhất:

- DB đúng nhưng token cũ

## 12. Test nào đang chứng minh logic này

## 12.1. `PermissionAuthorizationTests`

File: `ReviewFilms.Tests/PermissionAuthorizationTests.cs`

Đang kiểm tra:

- policy provider build đúng policy từ `HasPermission`
- handler deny khi thiếu claim permission
- handler succeed khi có đúng claim permission

Đây là bộ test trực tiếp nhất cho authorization layer.

## 12.2. `AuthServiceProfileTests`

File: `ReviewFilms.Tests/AuthServiceProfileTests.cs`

Đang kiểm tra:

- `GetCurrentUserProfileAsync(...)` trả đủ `Roles`
- `GetCurrentUserProfileAsync(...)` trả đủ `Permissions`

Đây là bộ test chứng minh service mapping từ DB sang DTO là đúng.

## 12.3. `AuthServiceRefreshTests`

File: `ReviewFilms.Tests/AuthServiceRefreshTests.cs`

Đang kiểm tra:

- refresh token rotation vẫn hoạt động
- `AuthResponse` sau refresh có `Permissions`
- JWT sau refresh chứa:
  - role claims
  - permission claims

Đây là bộ test chứng minh hybrid token flow hoạt động đúng.

## 12.4. `MoviesControllerSyncTests`

File: `ReviewFilms.Tests/MoviesControllerSyncTests.cs`

Đang kiểm tra:

- các write endpoint của `MoviesController` gắn đúng permission
- route template đúng
- `HasPermission` tạo đúng policy name

## 13. Cấu hình DI của permission authorization

Wiring chính ở `Extensions/AuthModuleExtensions.cs`:

- `services.AddAuthentication(...).AddJwtBearer(...)`
- `services.AddAuthorization()`
- `services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>()`
- `services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>()`

Điểm quan trọng:

- không sửa `Program.cs`
- `UseAuthentication()` được đưa vào pipeline qua `JwtAuthenticationStartupFilter`
- `Program.cs` chỉ cần giữ `app.UseAuthorization()` như hiện tại

## 14. Điều nên nhớ khi đọc code

Nếu bạn cần đọc theo đúng trình tự runtime, hãy đọc theo thứ tự này:

1. `Controllers/MoviesController.cs`
2. `Security/HasPermissionAttribute.cs`
3. `Security/PermissionAuthorizationPolicyProvider.cs`
4. `Security/PermissionRequirement.cs`
5. `Security/PermissionAuthorizationHandler.cs`
6. `Extensions/AuthModuleExtensions.cs`
7. `Services/AuthService.cs`
8. `Security/JwtTokenGenerator.cs`
9. `DTOs/Auth/AuthResponse.cs`
10. `DTOs/Auth/UserProfileDto.cs`

Nếu bạn cần đọc theo hướng dữ liệu đi từ DB đến token:

1. `AuthService.GetUserWithRolesAsync(...)`
2. `AuthService.GetDistinctPermissions(...)`
3. `AuthService.IssueTokensAsync(...)`
4. `JwtTokenGenerator.GenerateAccessToken(...)`
5. `PermissionAuthorizationHandler.HandleRequirementAsync(...)`

## 15. Lệnh test đã dùng để verify

```powershell
dotnet test ReviewFilms.Tests/ReviewFilms.Tests.csproj -c Release /p:UseAppHost=false
```

Kết quả tại thời điểm triển khai:

- `Passed: 26`
- `Failed: 0`

## 16. Kết luận ngắn

`JOB-012` đã biến authorization của module `Movies` từ kiểu role-based sang permission-based, nhưng vẫn giữ role claims trong JWT để tương thích ngược.

Khi debug, hãy luôn tách rõ 2 câu hỏi:

1. DB và service có đang trả đúng permission không?
2. token hiện tại có thật sự chứa permission đó không?

Nếu bạn trả lời được 2 câu này, phần lớn bug của luồng authorization sẽ được khoanh vùng rất nhanh.
