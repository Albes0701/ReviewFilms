# Postman Endpoint Module Template

Muc tieu cua file nay la chuan hoa tai lieu cho **tung endpoint** trong collection Postman. Moi file module rieng nen mo ta 1 endpoint duy nhat de de sinh request, pre-request script, test script, va matrix case.

## 1. Pham vi su dung

Dung template nay khi tao file moi trong `docs/postman/modules/**`.

- 1 file = 1 endpoint
- Folder Postman nen tach theo domain: `Auth`, `Movies`, `Reviews`, `Notifications`
- Request name nen ro nghia:
  - `Register`
  - `Login`
  - `Refresh`
  - `List`
  - `Get By Id`
  - `Create`
  - `Update`
  - `Delete`
  - `Mark As Read`
  - `Custom Action`

## 2. Cac section bat buoc trong moi file endpoint

```markdown
# [Domain] - [Endpoint Name]

## 1. Endpoint Summary
- Method:
- Route:
- Controller action:
- Auth requirement:
- Content-Type:
- Business purpose:

## 2. Source Of Truth
- Controller:
- Service:
- DTO Request:
- DTO Response:
- Middleware / Validation path:
- Enum / dependency lien quan:

## 3. Request Definition
### Path Params
### Query Params
### Headers
### Body / Form-Data
### Environment Variables doc / save

## 4. Expected Response
### Success
### Validation Error
### Business/Auth Error

## 5. Test Case Matrix
### Happy Path
### Validation
### Authentication
### Authorization / Ownership
### Business Rule
### Boundary / Edge
### State Chaining

## 6. Suggested Postman Setup
- Folder name
- Request name
- Pre-request script
- Tests script

## 7. Coverage Checklist
```

## 3. Environment Variables

Bien dung chung nen uu tien:

- `base_url`
- `access_token`
- `refresh_token`
- `current_user_id`
- `current_username`
- `current_email`
- `movie_id`
- `movie_slug`
- `movie_title`
- `movie_title_updated`
- `genre_id`
- `comment_id`
- `parent_comment_id`
- `notification_id`
- `notification_title`
- `notification_message`

Neu can du lieu unique, sinh bien trong pre-request script.

## 4. Response shape can nho

### Success response chung

Da so endpoint business tra:

```json
{
  "success": true,
  "message": "Success",
  "data": {}
}
```

### Validation error do `[ApiController]` + custom `InvalidModelStateResponseFactory`

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    "FieldName: error message"
  ]
}
```

### Business/auth error do `GlobalExceptionMiddleware`

```json
{
  "success": false,
  "message": "Concrete exception message",
  "errors": [
    "Concrete exception message"
  ]
}
```

## 5. Auth rules phai bám code that

- `POST /api/auth/refresh` dang nhan `accessToken` va `refreshToken` trong **body**
- Khong co refresh cookie flow trong code hien tai
- `Reviews` va `Notifications` duoc decorate `[Authorize]`
- `Movies` list/detail la public, nhung create/update van can user hop le vi service goi `ICurrentUserService`

## 6. Enum va serialization notes

- `NotificationType` request/response dang string vi co `JsonStringEnumConverter`
- `MovieStatus` va `CommentStatus` co the serialize response dang so neu khong co converter global
- Query enum thuong co the gui bang ten (`Published`) hoac so (`1`), nhung nen thong nhat 1 cach trong collection
- Voi `Guid`/`enum` non-nullable, `[Required]` khong phai luc nao cung bat duoc case "bo trong field"; can tach ro:
  - field bi bo trong va binder gan default value
  - field bi gui sai literal / sai kieu
  - field hop le ve kieu nhung tham chieu den resource khong ton tai

## 7. Multipart / Form-Data notes

Voi endpoint movie create/update:

- `Content-Type`: `multipart/form-data`
- `GenreIds` nen gui bang nhieu key lap lai:

```text
genreIds: {{genre_id}}
genreIds: {{genre_id_2}}
```

- File fields:
  - `posterFile`
  - `backdropFile`

## 8. Pre-request Script Template

```javascript
const suffix = Date.now().toString();

pm.environment.set("movie_title", `movie_${suffix}`);
pm.environment.set("movie_slug", `movie-${suffix}`);
pm.environment.set("current_username", `user_${suffix}`);
pm.environment.set("current_email", `user_${suffix}@example.com`);
pm.environment.set("notification_title", `notification_${suffix}`);
pm.environment.set("notification_message", `message_${suffix}`);
```

## 9. Success Test Script Template

```javascript
const json = pm.response.json();

pm.test("Status is success", function () {
  pm.expect(pm.response.code).to.be.oneOf([200, 201]);
});

pm.test("Response success is true", function () {
  pm.expect(json.success).to.eql(true);
});

pm.test("Response contains data", function () {
  pm.expect(json.data).to.exist;
});
```

## 10. Validation Error Test Script Template

```javascript
const json = pm.response.json();

pm.test("Status is 400", function () {
  pm.response.to.have.status(400);
});

pm.test("Validation failed shape is returned", function () {
  pm.expect(json.success).to.eql(false);
  pm.expect(json.message).to.eql("Validation failed.");
  pm.expect(json.errors).to.be.an("array").that.is.not.empty;
});
```

## 11. Business/Auth Error Test Script Template

```javascript
const json = pm.response.json();

pm.test("Status is expected business error", function () {
  pm.expect(pm.response.code).to.be.oneOf([401, 404, 409]);
});

pm.test("Error envelope is returned", function () {
  pm.expect(json.success).to.eql(false);
  pm.expect(json.message).to.be.a("string").and.not.empty;
  pm.expect(json.errors).to.be.an("array").that.is.not.empty;
});
```

## 12. Save Variables Template

### Save id

```javascript
if (json.data && json.data.id) {
  pm.environment.set("resource_id", json.data.id);
}
```

### Save auth payload

```javascript
if (json.data && json.data.token) {
  pm.environment.set("access_token", json.data.token);
}

if (json.data && json.data.refreshToken) {
  pm.environment.set("refresh_token", json.data.refreshToken);
}

if (json.data && json.data.userId) {
  pm.environment.set("current_user_id", json.data.userId);
}
```

## 13. Mau test case block

```markdown
- Case ID: AUTH_LOGIN_01
- Type: Verified from code
- Scenario: Login bang username hop le
- Preconditions:
  - User da ton tai
- Input:
  {
    "usernameOrEmail": "{{current_username}}",
    "password": "{{auth_password}}"
  }
- Expected status: 200
- Expected assertions:
  - `success = true`
  - `data.token` ton tai
  - `data.refreshToken` ton tai
- Environment updates:
  - save `access_token`
  - save `refresh_token`
```

## 14. Checklist khi them file endpoint moi

- Route co khop controller that khong
- DTO body/query/form-data co khop source khong
- Co ghi ro `Content-Type`
- Co tach case `Verified from code` va `Exploratory`
- Co case `happy path`
- Co case `validation`
- Co case `auth/ownership` neu endpoint can
- Co case `not found/conflict`
- Co case boundary khi DTO co length/range/paging
- Co test script de save `id`/`token` neu endpoint tra ve du lieu can tai su dung
