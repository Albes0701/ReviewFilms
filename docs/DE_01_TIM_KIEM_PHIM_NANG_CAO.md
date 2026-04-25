# Đề 01: Tìm Kiếm Phim Nâng Cao

## 1. Mục tiêu
Mở rộng API danh sách phim hiện có để hỗ trợ tìm kiếm nâng cao theo nhiều tiêu chí đồng thời, phù hợp với nội dung về Routing, `[FromQuery]`, `IQueryable`, phân trang và lọc động trong phần 2 của báo cáo.

## 2. Yêu cầu đề bài
Hiện tại hệ thống đã có endpoint `GET /api/movies` để lấy danh sách phim. Sinh viên cần mở rộng endpoint này để hỗ trợ thêm các tiêu chí lọc sau:

- `search`: tìm theo tên phim hoặc original title.
- `genreId`: lọc theo thể loại.
- `personId`: lọc theo diễn viên hoặc đạo diễn tham gia.
- `status`: lọc theo trạng thái phim.
- `minRating`: chỉ lấy phim có điểm trung bình lớn hơn hoặc bằng giá trị truyền vào.
- `fromYear` và `toYear`: lọc theo khoảng năm phát hành.
- `sortBy`: hỗ trợ sắp xếp theo `releaseDate`, `rating`, `createdAt`.
- `sortDirection`: hỗ trợ `asc` hoặc `desc`.

## 3. Dữ liệu đầu vào
Client gửi request theo dạng query string. Ví dụ:

```http
GET /api/movies?pageNumber=1&pageSize=10&search=batman&genreId=GUID&minRating=7&fromYear=2015&toYear=2025&sortBy=rating&sortDirection=desc
```

## 4. Kết quả mong đợi
API phải trả về dữ liệu phân trang theo định dạng thống nhất của hệ thống, bao gồm:

- danh sách phim thỏa điều kiện
- tổng số bản ghi
- số trang hiện tại
- tổng số trang

Nếu không có dữ liệu phù hợp thì trả về mảng rỗng nhưng vẫn đúng cấu trúc `ApiResponse<PagedResult<MovieDto>>`.

## 5. Yêu cầu kỹ thuật bắt buộc
- Sử dụng `IQueryable` để nối điều kiện lọc động.
- Chỉ thực thi truy vấn một cách tối ưu ở tầng database.
- Không được load toàn bộ dữ liệu lên RAM rồi mới lọc.
- Giữ nguyên kiến trúc Controller -> Service -> DbContext.
- Không trả Entity trực tiếp ra ngoài.

## 6. Gợi ý chấm điểm
- Đúng endpoint và nhận đúng query params.
- Lọc đúng theo nhiều điều kiện kết hợp.
- Phân trang đúng.
- Sắp xếp đúng.
- Code đúng chuẩn async/await và tách lớp hợp lý.