# Đề 03: Bình Chọn Comment

## 1. Mục tiêu
Mở rộng module cộng đồng bằng cách cho phép người dùng upvote hoặc downvote bình luận, áp dụng đúng kiến thức về nghiệp vụ, EF Core và cập nhật dữ liệu quan hệ.

## 2. Yêu cầu đề bài
Sinh viên cần xây dựng chức năng vote cho comment với các quy tắc:

- Mỗi người dùng chỉ được vote một lần trên một comment.
- Nếu vote cùng loại lần nữa thì hệ thống bỏ vote.
- Nếu đổi từ upvote sang downvote hoặc ngược lại thì hệ thống cập nhật lại.
- Sau mỗi lần vote, phải cập nhật lại các trường:
  - `UpvoteCount`
  - `DownvoteCount`
  - `Score`

## 3. Endpoint gợi ý
- `POST /api/reviews/comments/{commentId}/vote`
- `DELETE /api/reviews/comments/{commentId}/vote`

## 4. Dữ liệu đầu vào

```json
{
  "voteType": "Up"
}
```

## 5. Kết quả mong đợi
- Hệ thống ghi nhận vote đúng theo người dùng hiện tại.
- Điểm số comment được cập nhật chính xác.
- Trả về dữ liệu comment sau khi vote hoặc response xác nhận thao tác thành công.

## 6. Yêu cầu kỹ thuật bắt buộc
- Chỉ user đã đăng nhập mới được vote.
- Không được dùng SQL viết tay.
- Dùng EF Core để cập nhật dữ liệu.
- Xử lý đúng các trường hợp comment không tồn tại hoặc comment đã bị ẩn.

## 7. Gợi ý chấm điểm
- Đúng logic 1 người 1 vote.
- Đúng cập nhật score.
- Code sạch, async, và đúng kiến trúc.