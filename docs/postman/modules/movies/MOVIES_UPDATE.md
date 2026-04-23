# Movies - Update

## 1. Endpoint Summary

- Method: `PUT`
- Route: `/api/movies/{id}`
- Controller action: `MoviesController.UpdateMovie`
- Auth requirement: runtime required via `ICurrentUserService`
- Content-Type: `multipart/form-data`
- Business purpose: cap nhat movie ton tai, co the doi slug, genre, file, va metadata

## 2. Source Of Truth

- Controller: `Controllers/MoviesController.cs`
- Service: `Services/MovieService.cs` -> `UpdateMovieAsync`
- DTO Request: `DTOs/Films/MovieUpdateRequest.cs`
- DTO Response: `DTOs/Films/MovieDto.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Path params

- `id` - GUID cua movie

### Headers

- Authorization: `Bearer {{access_token}}`
- Content-Type: `multipart/form-data`

### Form-data notes

- Tat ca field deu optional
- Neu co `title` nhung khong co `slug`, service se regenerate slug tu `title`
- Neu co `genreIds`, service thay the toan bo danh sach genre hien tai

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Movie updated successfully.`
- Core assertions:
  - `success = true`
  - `data.id = {{movie_id}}`
  - cac field da doi khop voi input moi

### Error

- `id` sai GUID -> `400`
- Movie khong ton tai -> `404`
- Khong co token -> `401`
- `genreIds` chua id khong ton tai -> `404`
- field dai qua constraint -> `400`

## 5. Test Case Matrix

### Happy Path

- `MOVIES_UPDATE_01` - Verified from code
  - Scenario: cap nhat `title`
  - Expected: `200`, `data.title` doi theo input

- `MOVIES_UPDATE_02` - Verified from code
  - Scenario: cap nhat `slug` thu cong
  - Expected: `200`, `data.slug` bang slug moi sau normalize

- `MOVIES_UPDATE_03` - Verified from code
  - Scenario: cap nhat `title` nhung khong truyen `slug`
  - Expected: service tu regenerate slug

- `MOVIES_UPDATE_04` - Verified from code
  - Scenario: cap nhat `genreIds`
  - Expected: danh sach genre cu bi thay the

### Validation

- `MOVIES_UPDATE_05` - Verified from code
  - Scenario: `title` vuot 255 ky tu
  - Expected: `400`

- `MOVIES_UPDATE_06` - Verified from code
  - Scenario: `status` sai enum
  - Expected: `400`

### Authentication

- `MOVIES_UPDATE_07` - Verified from code
  - Scenario: khong gui token
  - Expected: `401`

### Business Rule

- `MOVIES_UPDATE_08` - Verified from code
  - Scenario: movie khong ton tai
  - Expected: `404`

- `MOVIES_UPDATE_09` - Verified from code
  - Scenario: `genreIds` chua id khong ton tai
  - Expected: `404`

- `MOVIES_UPDATE_10` - Verified from code
  - Scenario: slug moi bi trung
  - Expected: auto them suffix unique

### Boundary / Edge

- `MOVIES_UPDATE_11` - Verified from code
  - Scenario: gui `originalTitle`, `overview`, `ageRating`, `originalLanguage`, `trailerUrl` la chuoi rong
  - Expected: service trim va luu theo logic hien tai

- `MOVIES_UPDATE_12` - Exploratory
  - Scenario: gui `genreIds` rong
  - Expected: xoa het genre relation cua movie

## 6. Suggested Postman Setup

- Folder: `Movies`
- Request name: `Update`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Movie is updated", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Movie updated successfully.");
  pm.expect(json.data.id).to.eql(pm.environment.get("movie_id"));
});

if (json.data.slug) {
  pm.environment.set("movie_slug", json.data.slug);
}
```

## 7. Coverage Checklist

- [ ] Co case update title
- [ ] Co case update manual slug
- [ ] Co case auto regenerate slug
- [ ] Co case invalid movie id
- [ ] Co case missing token
- [ ] Co case invalid genre ids
