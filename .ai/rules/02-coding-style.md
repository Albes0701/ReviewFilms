# LUẬT STYLE CODE & CONVENTIONS (02-CODING-STYLE)

## 1. Naming Conventions
- **PascalCase:** Dùng cho Folders, Classes, Interfaces, Methods, và Public Properties.
- **camelCase với dấu `_`:** Dùng cho Private fields (VD: `_filmService`, `_dbContext`).
- **Interfaces:** Bắt buộc bắt đầu bằng chữ `I` (VD: `IAuthService`).

## 2. Async/Await (Bắt buộc)
- Mọi thao tác I/O (Database, External API) phải là bất đồng bộ.
- Trả về `Task<T>` hoặc `Task`.
- Hậu tố method phải có chữ `Async` (VD: `GetUserByIdAsync`).

## 3. Dependency Injection (DI)
- Tuyệt đối dùng **Constructor Injection**.
- Không dùng Service Locator pattern (`provider.GetService()`) trừ trường hợp bất khả kháng trong Middleware/Background Service.

## 4. LINQ & Data Access
- Sử dụng **Method Syntax** (VD: `_context.Movies.Where(...).Select(...)`).
- Tránh dùng Query Syntax (VD: `from m in _context.Movies...`).
- Tối ưu truy vấn, tránh N+1 query bằng cách dùng `.Include()` hợp lý nhưng không được gây lặp vô hạn (Circular Reference) khi serialize JSON.