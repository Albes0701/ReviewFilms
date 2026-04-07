# Auth - Register

## 1. Endpoint Summary

- Method: `POST`
- Route: `/api/auth/register`
- Controller action: `AuthController.Register`
- Auth requirement: none
- Content-Type: `application/json`
- Business purpose: tao user moi, cap access token + refresh token ngay sau khi dang ky

## 2. Source Of Truth

- Controller: `Controllers/AuthController.cs`
- Service: `Services/AuthService.cs` -> `RegisterAsync`
- DTO Request: `DTOs/Auth/RegisterRequest.cs`
- DTO Response: `DTOs/Auth/AuthResponse.cs`
- Validation path: `Extensions/ServiceCollectionExtensions.cs`
- Error mapping: `Middlewares/GlobalExceptionMiddleware.cs`
- Dependency note: bang `roles` phai co role code `USER`

## 3. Request Definition

### Headers

- `Content-Type: application/json`

### Body

```json
{
  "username": "{{current_username}}",
  "email": "{{current_email}}",
  "password": "{{auth_password}}",
  "displayName": "Test User"
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
  - `current_username`
  - `current_email`

## 4. Expected Response

### Success

- HTTP status: `200`
- Message: `Registration successful.`
- Core assertions:
  - `success = true`
  - `data.token` la string
  - `data.refreshToken` la string
  - `data.userId` ton tai
  - `data.username` la username da normalize lowercase
  - `data.email` la email da normalize lowercase
  - `data.roles` chua `USER`

### Validation error

- HTTP status: `400`
- Message: `Validation failed.`
- Shape: `success`, `message`, `errors[]`

### Business error

- Duplicate username/email -> `409`
- Default role `USER` khong ton tai -> `409`

## 5. Test Case Matrix

### Happy Path

- `AUTH_REGISTER_01` - Verified from code
  - Scenario: dang ky voi body hop le toi thieu
  - Input: `username`, `email`, `password`
  - Expected: `200`, token + refresh token duoc tra ve

- `AUTH_REGISTER_02` - Verified from code
  - Scenario: dang ky voi `displayName`
  - Expected: `200`, `data.displayName` bang gia tri da trim

### Validation

- `AUTH_REGISTER_03` - Verified from code
  - Scenario: thieu `username`
  - Expected: `400`, `message = Validation failed.`

- `AUTH_REGISTER_04` - Verified from code
  - Scenario: thieu `email`
  - Expected: `400`

- `AUTH_REGISTER_05` - Verified from code
  - Scenario: thieu `password`
  - Expected: `400`

- `AUTH_REGISTER_06` - Verified from code
  - Scenario: `email` sai format
  - Expected: `400`

- `AUTH_REGISTER_07` - Verified from code
  - Scenario: `password` ngan hon 8 ky tu
  - Expected: `400`

- `AUTH_REGISTER_08` - Verified from code
  - Scenario: `username` ngan hon 3 ky tu hoac dai hon 50
  - Expected: `400`

- `AUTH_REGISTER_09` - Verified from code
  - Scenario: `displayName` dai hon 100
  - Expected: `400`

### Business Rule

- `AUTH_REGISTER_10` - Verified from code
  - Scenario: `username` bi trung
  - Expected: `409`, message xap xi `Username or email is already in use.`

- `AUTH_REGISTER_11` - Verified from code
  - Scenario: `email` bi trung
  - Expected: `409`

- `AUTH_REGISTER_12` - Exploratory
  - Scenario: bang `roles` khong co role `USER`
  - Expected: `409`

### Boundary / Edge

- `AUTH_REGISTER_13` - Verified from code
  - Scenario: `username` va `email` co uppercase + space dau/cuoi
  - Expected: response `username` va `email` da duoc lowercase + trim

- `AUTH_REGISTER_14` - Verified from code
  - Scenario: bo trong `displayName`
  - Expected: `displayName` fallback bang `request.Username.Trim()`

## 6. Suggested Postman Setup

- Folder: `Auth`
- Request name: `Register`

### Pre-request script

```javascript
const suffix = Date.now().toString();

pm.environment.set("current_username", `user_${suffix}`);
pm.environment.set("current_email", `user_${suffix}@example.com`);
pm.environment.set("auth_password", "Password123!");
```

### Tests script

```javascript
const json = pm.response.json();

pm.test("Status is 200", function () {
  pm.response.to.have.status(200);
});

pm.test("Registration envelope is valid", function () {
  pm.expect(json.success).to.eql(true);
  pm.expect(json.message).to.eql("Registration successful.");
  pm.expect(json.data.token).to.be.a("string").and.not.empty;
  pm.expect(json.data.refreshToken).to.be.a("string").and.not.empty;
  pm.expect(json.data.userId).to.be.a("string");
});

pm.environment.set("access_token", json.data.token);
pm.environment.set("refresh_token", json.data.refreshToken);
pm.environment.set("current_user_id", json.data.userId);
pm.environment.set("current_username", json.data.username);
pm.environment.set("current_email", json.data.email);
```

## 7. Coverage Checklist

- [ ] Co case happy path toi thieu
- [ ] Co case `displayName`
- [ ] Co case duplicate username/email
- [ ] Co case invalid email
- [ ] Co case password min length
- [ ] Co case trim/normalize
- [ ] Co script save token va user info

