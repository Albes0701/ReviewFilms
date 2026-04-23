# Reviews - Ratings Upsert

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/reviews/ratings`
- Controller action: `ReviewsController.UpsertRatingAsync`
- Auth requirement: `[Authorize]`
- Content-Type: `application/json`
- Business purpose: tao moi hoac cap nhat diem danh gia cua user hien tai cho 1 movie

## 2. Source Of Truth

- Controller: `Controllers/ReviewsController.cs`
- Service: `Services/ReviewService.cs` -> `UpsertRatingAsync`
- DTO Request: `DTOs/Reviews/RatingRequest.cs`
- Validation path: `Extensions/ServiceCollectionExtensions.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Headers

- Authorization: `Bearer {{access_token}}`
- `Content-Type: application/json`

### Body

```json
{
  "movieId": "{{movie_id}}",
  "score": 8
}
```

### Environment variables

- Read:
  - `access_token`
  - `movie_id`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Rating saved.`
- Core assertions:
  - `success = true`
  - `message = Rating saved.`
  - `data` co the `null`

### Validation error

- `score` ngoai `1..10` -> `400`
- `movieId` sai kieu / sai JSON literal -> `400`

### Auth / business error

- Khong co token -> `401`
- Token sai/het han -> `401`
- Movie khong ton tai -> `404`
- `movieId` bi bo trong va binder gan `Guid.Empty` -> can verify, nhung thuong se re vao nhanh `404`

## 5. Test Case Matrix

### Happy Path

- `REVIEWS_RATING_UPSERT_01` - Verified from code
  - Scenario: tao rating moi
  - Expected: `200`, message `Rating saved.`

- `REVIEWS_RATING_UPSERT_02` - Verified from code
  - Scenario: upsert lai cung movie voi score moi
  - Expected: `200`, van tra success thay vi tao ban ghi moi

### Validation

- `REVIEWS_RATING_UPSERT_03` - Verified from code
  - Scenario: `score = 0`
  - Expected: `400`

- `REVIEWS_RATING_UPSERT_04` - Verified from code
  - Scenario: `score = 11`
  - Expected: `400`

- `REVIEWS_RATING_UPSERT_05` - Exploratory
  - Scenario: `movieId` gui sai literal GUID trong JSON
  - Expected: `400`

### Authentication

- `REVIEWS_RATING_UPSERT_06` - Verified from code
  - Scenario: khong gui bearer token
  - Expected: `401`

- `REVIEWS_RATING_UPSERT_07` - Exploratory
  - Scenario: token sai format hoac het han
  - Expected: `401`

### Business Rule

- `REVIEWS_RATING_UPSERT_08` - Verified from code
  - Scenario: `movieId` khong ton tai
  - Expected: `404`, message xap xi `Movie '...' was not found.`

- `REVIEWS_RATING_UPSERT_09` - Exploratory
  - Scenario: bo trong `movieId` de binder dung `Guid.Empty`
  - Expected: thuong `404`

### Boundary / Edge

- `REVIEWS_RATING_UPSERT_10` - Verified from code
  - Scenario: `score = 1`
  - Expected: `200`

- `REVIEWS_RATING_UPSERT_11` - Verified from code
  - Scenario: `score = 10`
  - Expected: `200`

## 6. Suggested Postman Setup

- Folder: `Reviews`
- Request name: `Ratings - Upsert`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Rating save response is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Rating saved.");
});
```

## 7. Coverage Checklist

- [ ] Co case create rating
- [ ] Co case update rating
- [ ] Co case score lower bound
- [ ] Co case score upper bound
- [ ] Co case invalid range
- [ ] Co case missing token
- [ ] Co case movie not found
