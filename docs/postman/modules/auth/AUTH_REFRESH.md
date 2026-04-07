# Auth - Refresh

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/auth/refresh`
- Controller action: `AuthController.Refresh`
- Auth requirement: none
- Content-Type: `application/json`
- Business purpose: xoay vong refresh token va cap access token moi

## 2. Source Of Truth

- Controller: `Controllers/AuthController.cs`
- Service: `Services/AuthService.cs` -> `RefreshAsync`
- DTO Request: `DTOs/Auth/RefreshRequest.cs`
- DTO Response: `DTOs/Auth/AuthResponse.cs`
- Validation path: `Extensions/ServiceCollectionExtensions.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`
- Regression reference: `ReviewFilms.Tests/AuthServiceRefreshTests.cs`

## 3. Request Definition

### Important note

- Endpoint nay **khong doc cookie**
- Body phai chua:
  - `accessToken`
  - `refreshToken`

### Body

```json
{
  "accessToken": "{{access_token}}",
  "refreshToken": "{{refresh_token}}"
}
```

### Environment variables

- Read:
  - `access_token`
  - `refresh_token`
- Save:
  - `access_token`
  - `refresh_token`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Token refreshed successfully.`
- Core assertions:
  - `success = true`
  - `data.token` khac token cu
  - `data.refreshToken` khac refresh token cu
  - `data.expiresAt` ton tai

### Validation error

- Thieu `accessToken` -> `400`
- Thieu `refreshToken` -> `400`

### Auth error

- `accessToken` sai format -> `401`
- `accessToken` khong co `jti` hop le -> `401`
- `refreshToken` sai -> `401`
- Reuse refresh token cu sau khi rotate -> `401`
- User khong `Active` -> `401`

## 5. Test Case Matrix

### Happy Path

- `AUTH_REFRESH_01` - Verified from code
  - Scenario: refresh hop le bang token moi nhat
  - Expected: `200`, cap ca access token va refresh token moi

### Validation

- `AUTH_REFRESH_02` - Verified from code
  - Scenario: thieu `accessToken`
  - Expected: `400`

- `AUTH_REFRESH_03` - Verified from code
  - Scenario: thieu `refreshToken`
  - Expected: `400`

### Authentication

- `AUTH_REFRESH_04` - Verified from code
  - Scenario: `accessToken` khong phai JWT hop le
  - Expected: `401`, message xap xi `Invalid access token.`

- `AUTH_REFRESH_05` - Verified from code
  - Scenario: `refreshToken` sai
  - Expected: `401`, message xap xi `Invalid refresh token.`

- `AUTH_REFRESH_06` - Verified from code
  - Scenario: dung lai refresh token cu sau lan refresh truoc
  - Expected: `401`

- `AUTH_REFRESH_07` - Exploratory
  - Scenario: user bi khoa sau khi nhan token
  - Expected: `401`, `Account is not active.`

### Boundary / Edge

- `AUTH_REFRESH_08` - Verified from code
  - Scenario: kiem tra rotation
  - Expected: refresh token moi khac refresh token cu

## 6. Suggested Postman Setup

- Folder: `Auth`
- Request name: `Refresh`

### Tests script

```javascript
const previousAccessToken = pm.environment.get("access_token");
const previousRefreshToken = pm.environment.get("refresh_token");
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Refresh envelope is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Token refreshed successfully.");
  pm.expect(json.data.token).to.be.a("string").and.not.empty;
  pm.expect(json.data.refreshToken).to.be.a("string").and.not.empty;
});

pm.test("Tokens are rotated", function () {
  pm.expect(json.data.token).to.not.eql(previousAccessToken);
  pm.expect(json.data.refreshToken).to.not.eql(previousRefreshToken);
});

pm.environment.set("access_token", json.data.token);
pm.environment.set("refresh_token", json.data.refreshToken);
```

## 7. Coverage Checklist

- [ ] Co case success refresh
- [ ] Co case missing body fields
- [ ] Co case invalid JWT
- [ ] Co case wrong refresh token
- [ ] Co case refresh token reuse sau rotation

