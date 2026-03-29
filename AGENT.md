# VIBECODE MASTER AI INSTRUCTIONS (ROUTER)

**Project:** ReviewFilms API (ASP.NET Core 10.0 LTS, MySQL)
**Architecture:** Monolithic Layered Architecture

⛔ **LỆNH BẮT BUỘC (PRE-FLIGHT CHECK):**
Để tối ưu hóa Context Window và tránh Merge Conflict khi thi công song song nhiều Worktree, toàn bộ luật lệ chi tiết đã được phân rã. 
BẠN BẮT BUỘC PHẢI SỬ DỤNG TOOL ĐỌC FILE ĐỂ ĐỌC CÁC TÀI LIỆU DƯỚI ĐÂY **TRƯỚC KHI** PHÂN TÍCH YÊU CẦU HAY VIẾT BẤT KỲ DÒNG CODE NÀO:

### 1. ĐỌC LUẬT CHUNG (Luôn luôn phải đọc)
Bạn phải nắm rõ 3 bộ luật định hình kiến trúc của dự án này:
- 📄 Đọc file: `.ai/rules/01-architecture.md` (Hiểu về các Layer và DI)
- 📄 Đọc file: `.ai/rules/02-coding-style.md` (Quy chuẩn đặt tên, Async, LINQ)
- 📄 Đọc file: `.ai/rules/03-parallel-merge.md` (Quy tắc sinh code an toàn, không sửa Program.cs)

### 2. ĐỌC NGỮ CẢNH MODULE (Chỉ đọc file liên quan đến task được giao)
Tùy thuộc vào yêu cầu (prompt) của tôi thuộc module nào, hãy đọc ĐÚNG file context của module đó để biết cấu trúc Database và DTOs tương ứng:
- 📦 Nếu làm về Auth/Security ➔ Đọc file: `.ai/modules/auth-context.md`
- 📦 Nếu làm về Film/Media ➔ Đọc file: `.ai/modules/film-context.md`
- 📦 Nếu làm về Review/Rating ➔ Đọc file: `.ai/modules/review-context.md`
- 📦 Nếu làm về Notification ➔ Đọc file: `.ai/modules/noti-context.md`

⚠️ **CẢNH BÁO TỐI THƯỢNG:** Tuyệt đối KHÔNG ĐƯỢC đoán mò Database Schema. KHÔNG ĐƯỢC tự ý bịa ra cấu trúc DTO nếu chưa đọc file context. Mọi Entity đã được tạo sẵn bằng EF Core MySQL, bạn tuyệt đối không được sửa đổi file trong thư mục `/Entities`.