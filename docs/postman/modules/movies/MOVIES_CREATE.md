# Movies - Create

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/movies`
- Controller action: `MoviesController.CreateMovie`
- Auth requirement: runtime required via `ICurrentUserService`
- Content-Type: `multipart/form-data`
- Business purpose: tao movie moi, auto tao slug neu can, upload poster/backdrop neu co

## 2. Source Of Truth

- Controller: `Controllers/MoviesController.cs`
- Service: `Services/MovieService.cs` -> `CreateMovieAsync`
- DTO Request: `DTOs/Films/MovieCreateRequest.cs`
- DTO Response: `DTOs/Films/MovieDto.cs`
- Validation path: `Extensions/ServiceCollectionExtensions.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`
- Regression reference: `ReviewFilms.Tests/MovieServiceTests.cs`

## 3. Request Definition

### Headers

- Authorization: `Bearer {{access_token}}`
- Content-Type: `multipart/form-data`

### Form-data fields

- `title` - required, max 255
- `originalTitle` - optional, max 255
- `slug` - optional, max 255
- `overview` - optional
- `releaseDate` - optional, `yyyy-MM-dd`
- `runtimeMinutes` - optional
- `ageRating` - optional, max 20
- `originalLanguage` - optional, max 10
- `trailerUrl` - optional, max 500
- `status` - optional, prefer `Draft`/`Published`/`Archived`
- `posterFile` - optional file
- `backdropFile` - optional file
- `genreIds` - optional, repeat key de gui nhieu GUID

### Minimal example

```text
title: {{movie_title}}
status: Published
```

## 4. Expected Response

### Success

- HTTP status: `201`
- Message: `Movie created successfully.`
- Core assertions:
  - `success = true`
  - `data.id` ton tai
  - `data.title = {{movie_title}}`
  - `data.slug` ton tai
  - `data.createdByUserId = {{current_user_id}}`

### Validation error

- Thieu `title` -> `400`
- `title` > 255 -> `400`
- `slug` > 255 -> `400`
- `ageRating` > 20 -> `400`
- `originalLanguage` > 10 -> `400`
- `trailerUrl` > 500 -> `400`

### Business/Auth error

- Khong co token / token sai -> `401` tu `CurrentUserService`
- `genreIds` chua GUID khong ton tai -> `404`
- `slug` sau normalize rong -> `400`

## 5. Test Case Matrix

### Happy Path

- `MOVIES_CREATE_01` - Verified from code
  - Scenario: tao movie voi field toi thieu
  - Expected: `201`, tra ve `id`, `slug`, `createdByUserId`

- `MOVIES_CREATE_02` - Verified from code
  - Scenario: tao movie voi full form-data va `genreIds`
  - Expected: `201`, `genres` duoc map ra response detail

- `MOVIES_CREATE_03` - Verified from code
  - Scenario: khong truyen `slug`
  - Expected: service tu normalize tu `title`

### Validation

- `MOVIES_CREATE_04` - Verified from code
  - Scenario: thieu `title`
  - Expected: `400`

- `MOVIES_CREATE_05` - Verified from code
  - Scenario: `title` vuot 255 ky tu
  - Expected: `400`

- `MOVIES_CREATE_06` - Verified from code
  - Scenario: `status` sai enum
  - Expected: `400` hoac model binding error

### Authentication

- `MOVIES_CREATE_07` - Verified from code
  - Scenario: khong gui bearer token
  - Expected: `401`, message xap xi `User is not authenticated.`

### Business Rule

- `MOVIES_CREATE_08` - Verified from code
  - Scenario: `genreIds` co id khong ton tai
  - Expected: `404`

- `MOVIES_CREATE_09` - Verified from code
  - Scenario: slug bi trung
  - Expected: service auto them hau to `-2`, `-3`, ...

### Boundary / Edge

- `MOVIES_CREATE_10` - Verified from code
  - Scenario: `title` co ky tu dac biet
  - Expected: `slug` duoc normalize bo ky tu khong hop le

- `MOVIES_CREATE_11` - Exploratory
  - Scenario: `title` chi gom khoang trang
  - Expected: co the qua validation model nhung fail o service khi slug rong -> `400`

## 6. Suggested Postman Setup

- Folder: `Movies`
- Request name: `Create`

### Pre-request script

```javascript
const suffix = Date.now().toString();

pm.environment.set("movie_title", `movie_${suffix}`);
pm.environment.set("movie_title_updated", `movie_updated_${suffix}`);
pm.environment.set("movie_slug", `movie-${suffix}`);
```

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 201", function () {
  pm.response.to.have.status(201);
});

pm.test("Movie is created", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Movie created successfully.");
  pm.expect(json.data.id).to.be.a("string");
  pm.expect(json.data.slug).to.be.a("string").and.not.empty;
});

pm.environment.set("movie_id", json.data.id);
pm.environment.set("movie_slug", json.data.slug);
```

## 7. Coverage Checklist

- [ ] Co case create toi thieu
- [ ] Co case full form-data
- [ ] Co case auth missing
- [ ] Co case invalid genre
- [ ] Co case auto slug / duplicate slug
- [ ] Co save `movie_id`

