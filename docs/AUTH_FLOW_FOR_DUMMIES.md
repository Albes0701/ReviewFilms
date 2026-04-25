# 🏰 Giải thích Luồng Request (Cho Bò Cũng Hiểu)

Hãy tưởng tượng hệ thống của chúng ta là một **Rạp Chiếu Phim**.

## 1. Biểu đồ luồng (Sequence Diagram)

```mermaid
sequenceDiagram
    participant U as User (Khách)
    participant API as Cổng rạp (Middleware)
    participant Auth as Soát vé (Authentication)
    participant Perm as Kiểm thẻ VIP (Authorization)
    participant Ctrl as Phòng chiếu/Kho (Controller)

    Note over U, Ctrl: TRƯỜNG HỢP 1: KHÔNG CẦN AUTH (Xem Poster)
    U->>API: Cho em xem poster ngoài sảnh
    API->>Ctrl: Cửa mở sẵn, vào xem đi!
    Ctrl-->>U: Trả về hình ảnh poster

    Note over U, Ctrl: TRƯỜNG HỢP 2: CẦN AUTH + PERMISSION (Đăng Phim)
    U->>API: Em muốn vào kho đăng phim mới
    API->>Auth: Đâu, vé (Token) đâu?
    alt Không có vé/Vé giả
        Auth-->>U: Đuổi về ngay (401 Unauthorized)
    else Có vé xịn
        Auth->>Perm: Có vé rồi, xem trên vé có đóng dấu "ĐƯỢC ĐĂNG PHIM" không?
        alt Không có dấu (Permission)
            Perm-->>U: Có vé vào rạp nhưng tuổi gì vào kho! (403 Forbidden)
        else Có dấu "movies:create"
            Perm->>Ctrl: Đủ quyền rồi, mời sếp vào kho!
            Ctrl-->>U: Xong! Phim đã được đăng.
        end
    end
```

---

## 2. Giải thích siêu ngắn gọn

Hệ thống hoạt động qua 3 "vòng gửi xe":

### 🔹 Vòng 1: Tự do (No Auth)
- **Dấu hiệu:** Không thấy chữ `[Authorize]` hay `[HasPermission]` trên đầu hàm (Method).
- **Cách chạy:** Bạn cứ đi thẳng vào. Giống như đi bộ ngoài sảnh rạp phim để xem lịch chiếu, chẳng ai hỏi thẻ hay vé gì cả.

### 🔹 Vòng 2: Phải có vé (Authentication - 401)
- **Dấu hiệu:** Có chữ `[Authorize]`.
- **Cách chạy:** Ông bảo vệ sẽ chặn lại hỏi: "Bạn là ai? Cho xem vé (JWT Token)".
    - **Không có vé:** Đuổi về lỗi **401**.
    - **Có vé:** Cho qua nhưng chưa chắc được làm mọi thứ.

### 🔹 Vòng 3: Phải có quyền đặc biệt (Authorization - 403)
- **Dấu hiệu:** Có chữ `[HasPermission("tên_quyền")]`.
- **Cách chạy:** Sau khi xem vé xong, ông bảo vệ nhìn kỹ hơn xem trên vé có ghi quyền cụ thể không (ví dụ: `movies:create`, `movies:delete`).
    - **Không có quyền:** Bảo vệ bảo "Vé này chỉ để xem thôi, không được sửa!" -> Lỗi **403**.
    - **Có đúng quyền:** Mời vào làm việc!

---

## 3. Tóm tắt Code thực tế

Nhìn vào `MoviesController.cs`:

1.  **Hàm `GetMovies`:** Không có `[HasPermission]`.
    - ➡️ Bò vào xem thoải mái.
2.  **Hàm `CreateMovie`:** Có `[HasPermission("movies:create")]`.
    - ➡️ Bò phải có **Vé (Token)** + Trong vé phải ghi là được quyền **"movies:create"**.
