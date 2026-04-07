# Reviews - Comments Create

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/reviews/comments`
- Controller action: `ReviewsController.CreateCommentAsync`
- Auth requirement: `[Authorize]`
- Content-Type: `application/json`
- Business purpose: tao root comment hoac reply comment cho movie

## 2. Source Of Truth

- Controller: `Controllers/ReviewsController.cs`
- Service: `Services/ReviewService.cs` -> `CreateCommentAsync`
- DTO Request: `DTOs/Reviews/CommentRequest.cs`
- DTO Response: `DTOs/Reviews/CommentResponse.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`
- Regression reference: `ReviewFilms.Tests/ReviewServiceTests.cs`

## 3. Request Definition

### Headers

- Authorization: `Bearer {{access_token}}`
- `Content-Type: application/json`

### Body - root comment

```json
{
  "movieId": "{{movie_id}}",
  "content": "{{comment_content}}"
}
```

### Body - reply comment

```json
{
  "movieId": "{{movie_id}}",
  "content": "Reply comment",
  "parentId": "{{parent_comment_id}}"
}
```

### Environment variables

- Read:
  - `access_token`
  - `movie_id`
  - `comment_content`
  - `parent_comment_id`
- Save:
  - `comment_id`
  - `parent_comment_id` (neu muon chain reply)

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Comment created.`
- Core assertions:
  - `data.id` ton tai
  - `data.movieId = {{movie_id}}`
  - `data.content` bang noi dung da trim
  - `data.childComments` la array

### Validation error

- Thieu `content` -> `400`
- `content` > 4000 -> `400`
- `movieId` sai kieu / sai JSON literal -> `400`

### Auth / business error

- Khong co token -> `401`
- Movie khong ton tai -> `404`
- `parentId` khong ton tai trong movie do -> `404`
- Parent comment khong visible -> `409`
- `movieId` bi bo trong va binder gan `Guid.Empty` -> can verify, nhung thuong re vao nhanh `404`

## 5. Test Case Matrix

### Happy Path

- `REVIEWS_COMMENT_CREATE_01` - Verified from code
  - Scenario: tao root comment
  - Expected: `200`, `parentId = null`, `rootId = id`, `depth = 0`

- `REVIEWS_COMMENT_CREATE_02` - Verified from code
  - Scenario: tao reply cho root comment
  - Expected: `200`, `parentId` bang root comment, `rootId` bang root comment id, `depth = 1`

- `REVIEWS_COMMENT_CREATE_03` - Verified from code
  - Scenario: tao reply cho reply comment
  - Expected: `200`, `rootId` giu nguyen root ban dau, `depth` tang len

### Validation

- `REVIEWS_COMMENT_CREATE_04` - Verified from code
  - Scenario: thieu `content`
  - Expected: `400`

- `REVIEWS_COMMENT_CREATE_05` - Verified from code
  - Scenario: `content` dai hon 4000
  - Expected: `400`

- `REVIEWS_COMMENT_CREATE_06` - Exploratory
  - Scenario: `movieId` gui sai literal GUID trong JSON
  - Expected: `400`

### Authentication

- `REVIEWS_COMMENT_CREATE_07` - Verified from code
  - Scenario: khong gui bearer token
  - Expected: `401`

### Business Rule

- `REVIEWS_COMMENT_CREATE_08` - Verified from code
  - Scenario: movie khong ton tai
  - Expected: `404`

- `REVIEWS_COMMENT_CREATE_09` - Verified from code
  - Scenario: `parentId` khong ton tai trong movie do
  - Expected: `404`

- `REVIEWS_COMMENT_CREATE_10` - Verified from code
  - Scenario: reply vao comment cua user khac
  - Expected: `200`, va flow tiep theo co the kiem tra notification duoc tao

- `REVIEWS_COMMENT_CREATE_11` - Verified from code
  - Scenario: self-reply
  - Expected: `200`, khong tao notification moi

- `REVIEWS_COMMENT_CREATE_12` - Exploratory
  - Scenario: parent comment co `Status != Visible`
  - Expected: `409`, `Parent comment is not available for replies.`

- `REVIEWS_COMMENT_CREATE_13` - Exploratory
  - Scenario: bo trong `movieId` de binder dung `Guid.Empty`
  - Expected: thuong `404`

### Boundary / Edge

- `REVIEWS_COMMENT_CREATE_14` - Verified from code
  - Scenario: `content` chi co space dau/cuoi
  - Expected: noi dung duoc trim truoc khi luu

## 6. Suggested Postman Setup

- Folder: `Reviews`
- Request name: `Comments - Create`

### Pre-request script

```javascript
const suffix = Date.now().toString();
pm.environment.set("comment_content", `comment_${suffix}`);
```

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Comment response is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Comment created.");
  pm.expect(json.data.id).to.be.a("string");
  pm.expect(json.data.childComments).to.be.an("array");
});

pm.environment.set("comment_id", json.data.id);

if (!pm.environment.get("parent_comment_id")) {
  pm.environment.set("parent_comment_id", json.data.id);
}
```

## 7. Coverage Checklist

- [ ] Co case root comment
- [ ] Co case reply comment
- [ ] Co case nested reply
- [ ] Co case movie not found
- [ ] Co case invalid parentId
- [ ] Co case missing token
- [ ] Co case trim content
