# Reviews - Ratings Delete

## 1. Endpoint Summary

- Method: `DELETE`
- Route: `/api/reviews/ratings/{movieId}`
- Controller action: `ReviewsController.DeleteRatingAsync`
- Auth requirement: `[Authorize]`
- Content-Type: path param
- Business purpose: xoa rating cua user hien tai cho 1 movie

## 2. Source Of Truth

- Controller: `Controllers/ReviewsController.cs`
- Service: `Services/ReviewService.cs` -> `DeleteRatingAsync`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Path params

- `movieId` - GUID cua movie da duoc rate boi current user

### Headers

- Authorization: `Bearer {{access_token}}`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Rating deleted.`
- `data` co the `null`

### Error

- `movieId` sai GUID -> `400`
- Khong co token -> `401`
- Movie khong ton tai -> `404`
- Rating cua user hien tai khong ton tai -> `404`

## 5. Test Case Matrix

### Happy Path

- `REVIEWS_RATING_DELETE_01` - Verified from code
  - Scenario: xoa rating da ton tai
  - Expected: `200`, message `Rating deleted.`

### Validation

- `REVIEWS_RATING_DELETE_02` - Verified from code
  - Scenario: `movieId` sai format GUID
  - Expected: `400`

### Authentication

- `REVIEWS_RATING_DELETE_03` - Verified from code
  - Scenario: thieu bearer token
  - Expected: `401`

### Business Rule

- `REVIEWS_RATING_DELETE_04` - Verified from code
  - Scenario: movie khong ton tai
  - Expected: `404`

- `REVIEWS_RATING_DELETE_05` - Verified from code
  - Scenario: rating khong ton tai cho user hien tai
  - Expected: `404`, message xap xi `Rating for movie '...' and user '...' was not found.`

### Boundary / Edge

- `REVIEWS_RATING_DELETE_06` - Verified from code
  - Scenario: xoa rating lan 2 lien tiep
  - Expected: lan 1 `200`, lan 2 `404`

## 6. Suggested Postman Setup

- Folder: `Reviews`
- Request name: `Ratings - Delete`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Rating delete response is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Rating deleted.");
});
```

## 7. Coverage Checklist

- [ ] Co case delete success
- [ ] Co case invalid guid
- [ ] Co case missing token
- [ ] Co case delete lai lan 2
- [ ] Co case rating not found

