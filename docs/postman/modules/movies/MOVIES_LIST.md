# Movies - List

## 1. Endpoint Summary

- Method: `GET`
- Route: `/api/movies`
- Controller action: `MoviesController.GetMovies`
- Auth requirement: public
- Content-Type: query string
- Business purpose: lay danh sach phim co paging va filter

## 2. Source Of Truth

- Controller: `Controllers/MoviesController.cs`
- Service: `Services/MovieService.cs` -> `GetMoviesAsync`
- DTO Response: `DTOs/Films/MovieDto.cs`, `DTOs/Common/PagedResult.cs`
- Enum: `Enums/MovieStatus.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Query params

- `pageNumber` - mac dinh `1`
- `pageSize` - mac dinh `10`, service clamp `1..100`
- `search` - tim theo `Title`, `OriginalTitle`, `Slug`
- `genreId` - loc theo genre
- `status` - `Draft`, `Published`, `Archived` hoac gia tri enum tuong ung

### Example

```text
GET {{base_url}}/api/movies?pageNumber=1&pageSize=10&search={{movie_title}}&status=Published
```

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Success`
- Core assertions:
  - `success = true`
  - `data.items` la array
  - `data.pageNumber`, `data.pageSize`, `data.totalCount`, `data.totalPages` ton tai
  - `data.hasPreviousPage`, `data.hasNextPage` ton tai

### Validation / binding error

- `status` sai enum -> `400`
- `genreId` sai GUID -> `400`

### Edge behavior

- `pageNumber <= 0` duoc clamp ve `1`
- `pageSize <= 0` duoc clamp thanh `1`
- `pageSize > 100` duoc clamp thanh `100`

## 5. Test Case Matrix

### Happy Path

- `MOVIES_LIST_01` - Verified from code
  - Scenario: list mac dinh khong filter
  - Expected: `200`

- `MOVIES_LIST_02` - Verified from code
  - Scenario: filter theo `search`
  - Expected: chi tra item co `title`, `originalTitle`, hoac `slug` khop

- `MOVIES_LIST_03` - Verified from code
  - Scenario: filter theo `genreId`
  - Expected: chi tra item co genre do

- `MOVIES_LIST_04` - Verified from code
  - Scenario: filter theo `status`
  - Expected: chi tra item dung status

- `MOVIES_LIST_05` - Verified from code
  - Scenario: ket hop `search + genreId + status`
  - Expected: `200`

### Validation

- `MOVIES_LIST_06` - Verified from code
  - Scenario: `genreId` sai format
  - Expected: `400`

- `MOVIES_LIST_07` - Verified from code
  - Scenario: `status` sai enum
  - Expected: `400`

### Boundary / Edge

- `MOVIES_LIST_08` - Verified from code
  - Scenario: `pageNumber=0`
  - Expected: response `data.pageNumber = 1`

- `MOVIES_LIST_09` - Verified from code
  - Scenario: `pageSize=0`
  - Expected: response `data.pageSize = 1`

- `MOVIES_LIST_10` - Verified from code
  - Scenario: `pageSize=999`
  - Expected: response `data.pageSize = 100`

- `MOVIES_LIST_11` - Exploratory
  - Scenario: khong co phim nao khop bo loc
  - Expected: `200`, `data.items = []`

## 6. Suggested Postman Setup

- Folder: `Movies`
- Request name: `List`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Paged response shape is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Success");
  pm.expect(json.data.items).to.be.an("array");
  pm.expect(json.data.pageNumber).to.be.a("number");
  pm.expect(json.data.pageSize).to.be.a("number");
});

if (json.data.items.length > 0) {
  pm.environment.set("movie_id", json.data.items[0].id);
  pm.environment.set("movie_slug", json.data.items[0].slug);
}
```

## 7. Coverage Checklist

- [ ] Co case mac dinh
- [ ] Co case search
- [ ] Co case genreId
- [ ] Co case status
- [ ] Co case invalid enum/guid
- [ ] Co case clamp page/pageSize

