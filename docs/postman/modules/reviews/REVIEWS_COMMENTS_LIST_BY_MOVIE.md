# Reviews - Comments List By Movie

## 1. Endpoint Summary

- Method: `GET`
- Route: `/api/reviews/movies/{movieId}/comments`
- Controller action: `ReviewsController.GetCommentsAsync`
- Auth requirement: `[Authorize]`
- Content-Type: path param
- Business purpose: lay cay comment visible cua 1 movie

## 2. Source Of Truth

- Controller: `Controllers/ReviewsController.cs`
- Service: `Services/ReviewService.cs` -> `GetCommentsAsync`
- DTO Response: `DTOs/Reviews/CommentResponse.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Path params

- `movieId` - GUID cua movie

### Headers

- Authorization: `Bearer {{access_token}}`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Comments loaded.`
- Core assertions:
  - `data` la array root comments
  - moi item co `childComments`
  - chi tra comment `Status = Visible`

### Error

- `movieId` sai GUID -> `400`
- Khong co token -> `401`
- Movie khong ton tai -> `404`

## 5. Test Case Matrix

### Happy Path

- `REVIEWS_COMMENT_LIST_01` - Verified from code
  - Scenario: movie co root comment va child comments
  - Expected: `200`, tree duoc long dung theo `parentId`

- `REVIEWS_COMMENT_LIST_02` - Exploratory
  - Scenario: movie ton tai nhung chua co comment
  - Expected: `200`, `data = []`

### Validation

- `REVIEWS_COMMENT_LIST_03` - Verified from code
  - Scenario: `movieId` sai format
  - Expected: `400`

### Authentication

- `REVIEWS_COMMENT_LIST_04` - Verified from code
  - Scenario: thieu bearer token
  - Expected: `401`

### Business Rule

- `REVIEWS_COMMENT_LIST_05` - Verified from code
  - Scenario: movie khong ton tai
  - Expected: `404`

### Boundary / Edge

- `REVIEWS_COMMENT_LIST_06` - Verified from code
  - Scenario: tao nhieu cap reply roi list lai
  - Expected: child comments duoc sort tang dan theo `createdAt`

## 6. Suggested Postman Setup

- Folder: `Reviews`
- Request name: `Comments - List By Movie`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Comments tree response is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Comments loaded.");
  pm.expect(json.data).to.be.an("array");
});
```

## 7. Coverage Checklist

- [ ] Co case list cay comment
- [ ] Co case empty comments
- [ ] Co case invalid guid
- [ ] Co case missing token
- [ ] Co case not found
- [ ] Co case sort child comments

