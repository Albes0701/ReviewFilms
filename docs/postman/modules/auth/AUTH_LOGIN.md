# Auth - Login

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/auth/login`
- Controller action: `AuthController.Login`
- Auth requirement: none
- Content-Type: `application/json`
- Business purpose: dang nhap bang username hoac email va nhan token moi

## 2. Source Of Truth

- Controller: `Controllers/AuthController.cs`
- Service: `Services/AuthService.cs` -> `LoginAsync`
- DTO Request: `DTOs/Auth/LoginRequest.cs`
- DTO Response: `DTOs/Auth/AuthResponse.cs`
- Validation path: `Extensions/ServiceCollectionExtensions.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`

## 3. Request Definition

### Body

```json
{
  "usernameOrEmail": "{{current_username}}",
  "password": "{{auth_password}}"
}
```

### Environment variables

- Read:
  - `current_username`
  - `current_email`
  - `auth_password`
- Save:
  - `access_token`
  - `refresh_token`
  - `current_user_id`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Login successful.`
- Core assertions:
  - `success = true`
  - `data.token` ton tai
  - `data.refreshToken` ton tai
  - `data.roles` la array

### Validation error

- Thieu `usernameOrEmail` -> `400`
- Thieu `password` -> `400`
- `usernameOrEmail` dai hon 255 -> `400`
- `password` dai hon 100 -> `400`

### Business/Auth error

- Username/email khong ton tai -> `401`
- Password sai -> `401`
- Tai khoan khong `Active` -> `401`

## 5. Test Case Matrix

### Happy Path

- `AUTH_LOGIN_01` - Verified from code
  - Scenario: login bang username hop le
  - Expected: `200`, token duoc refresh moi

- `AUTH_LOGIN_02` - Verified from code
  - Scenario: login bang email hop le
  - Expected: `200`

### Validation

- `AUTH_LOGIN_03` - Verified from code
  - Scenario: thieu `usernameOrEmail`
  - Expected: `400`

- `AUTH_LOGIN_04` - Verified from code
  - Scenario: thieu `password`
  - Expected: `400`

### Authentication

- `AUTH_LOGIN_05` - Verified from code
  - Scenario: password sai
  - Expected: `401`, message xap xi `Invalid username/email or password.`

- `AUTH_LOGIN_06` - Verified from code
  - Scenario: username/email khong ton tai
  - Expected: `401`

- `AUTH_LOGIN_07` - Exploratory
  - Scenario: tai khoan `Status != Active`
  - Expected: `401`

### Boundary / Edge

- `AUTH_LOGIN_08` - Verified from code
  - Scenario: gui username co uppercase + space dau/cuoi
  - Expected: login van thanh cong vi service normalize identifier

- `AUTH_LOGIN_09` - Verified from code
  - Scenario: gui email co uppercase + space dau/cuoi
  - Expected: login van thanh cong

## 6. Suggested Postman Setup

- Folder: `Auth`
- Request name: `Login`

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Login envelope is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Login successful.");
  pm.expect(json.data.token).to.be.a("string").and.not.empty;
  pm.expect(json.data.refreshToken).to.be.a("string").and.not.empty;
});

pm.environment.set("access_token", json.data.token);
pm.environment.set("refresh_token", json.data.refreshToken);
pm.environment.set("current_user_id", json.data.userId);
```

## 7. Coverage Checklist

- [ ] Co case login bang username
- [ ] Co case login bang email
- [ ] Co case wrong password
- [ ] Co case unknown user
- [ ] Co case missing fields
- [ ] Co case normalized identifier

