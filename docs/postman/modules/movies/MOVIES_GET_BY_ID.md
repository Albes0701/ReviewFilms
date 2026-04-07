# Movies - Get By Id

## 1. Endpoint Summary

- Method: `GET`
- Route: `/api/movies/{id}`
- Controller action: `MoviesController.GetMovieById`
- Auth requirement: public
- Content-Type: path param
- Business purpose: lay chi tiet 1 phim va relations `genres`, `credits`

## 2. Source Of Truth

- Controller: `Controllers/MoviesController.cs`
- Service: `Services/MovieService.cs` -> `GetMovieByIdAsync`
- DTO Response: `DTOs/Films/MovieDto.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Path params

- `id` - GUID cua movie

### Example

```text
GET {{base_url}}/api/movies/{{movie_id}}
```

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Success`
- Core assertions:
  - `data.id = {{movie_id}}`
  - `data.title` ton tai
  - `data.slug` ton tai
  - `data.genres` la array
  - `data.credits` la array

### Error

- Movie khong ton tai -> `404`
- `id` sai GUID -> `400`

## 5. Test Case Matrix

### Happy Path

- `MOVIES_GET_01` - Verified from code
  - Scenario: movie ton tai
  - Expected: `200`, co day du fields co ban va relations

### Validation

- `MOVIES_GET_02` - Verified from code
  - Scenario: `id` sai GUID
  - Expected: `400`

### Business Rule

- `MOVIES_GET_03` - Verified from code
  - Scenario: movie khong ton tai
  - Expected: `404`, message xap xi `Movie with id '...' was not found.`

### Boundary / Edge

- `MOVIES_GET_04` - Exploratory
  - Scenario: movie ton tai nhung khong co genres/credits
  - Expected: `200`, `genres = []`, `credits = []`

## 6. Suggested Postman Setup

- Folder: `Movies`
- Request name: `Get By Id`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Movie detail shape is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.data.id).to.eql(pm.environment.get("movie_id"));
  pm.expect(json.data.genres).to.be.an("array");
  pm.expect(json.data.credits).to.be.an("array");
});
```

## 7. Coverage Checklist

- [ ] Co case success
- [ ] Co case invalid guid
- [ ] Co case not found
- [ ] Co assert genres/credits

