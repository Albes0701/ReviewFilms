# Postman Endpoint Docs Design

**Goal:** Chuẩn hoa bo tai lieu Postman theo muc `1 overview + 1 file cho moi endpoint`, de nguoi tao collection co the sinh request va test script chi tiet dua tren source code thuc te cua ReviewFilms API.

## Scope

- Cap nhat `docs/postman/POSTMAN_MODULE_TEMPLATE.md` de phan anh dung request/response/error/auth flow cua code hien tai.
- Tao `docs/postman/POSTMAN_OVERVIEW.md` lam ban do tong quan cho env variables, naming, dependency chain, va file map.
- Tao tai lieu endpoint-level cho cac API business:
  - Auth: register, login, refresh
  - Movies: list, get by id, create, update
  - Reviews: upsert rating, delete rating, create comment, list comments by movie
  - Notifications: list, mark as read, create
- Loai bo `WeatherForecastController` khoi pham vi docs business.

## Architecture

Bo tai lieu se chia theo domain roi tieu muc theo endpoint, thay vi gop theo controller. Cac file docs se dung chung mot khuon dang: endpoint summary, source of truth, request definition, expected response, test case matrix, goi y Postman scripts, va coverage checklist.

Noi dung chi duoc xac nhan tu source code hien tai:

- Controller routes
- DTO validation
- Service business rules
- Global exception mapping
- Auth wiring va enum serialization pattern

Moi test case se duoc danh dau:

- `Verified from code`: suy ra truc tiep tu controller/DTO/service/middleware
- `Exploratory`: hop ly de thu, nhung co the phu thuoc du lieu DB, model binding multipart, hoac runtime config

## Design Decisions

### 1. One endpoint per file

- Moi file trong `docs/postman/modules/**` mo ta duy nhat 1 endpoint.
- Cach nay giup Postman folder/request naming ro rang va giu duoc test matrix day du.

### 2. Overview as execution map

- `POSTMAN_OVERVIEW.md` se la noi quy chuan hoa env vars, naming convention, route-to-file map, seed prerequisites, va end-to-end flow.
- File nay cung ghi ro cac nuance cua code hien tai, vi du:
  - `POST /api/auth/refresh` dung body, khong dung cookie
  - `Movies` list/detail la public, nhung create/update van can nguoi dung da dang nhap vi service goi `ICurrentUserService`
  - `Reviews` va `Notifications` yeu cau bearer token

### 3. Template aligned with source code

- Template se mo rong tu "module" sang "endpoint module".
- Them mau request/test script cho:
  - success response
  - validation error
  - business/auth error
  - luu env variables
- Them huong dan cho enum, multipart form-data, repeated `genreIds`, va error response shape.

## Risks

- Docs co the tro nen rat dai neu moi endpoint liet ke qua nhieu case lap lai. Giai phap la giu format co dinh va chi liet ke case co gia tri.
- `MovieStatus` va `CommentStatus` khong co `JsonStringEnumConverter`, nen response enum co kha nang serialize dang so. Docs can ghi ro de tranh tao assert sai.
- Mot so case can seed du lieu DB phu hop, dac biet voi `genreIds`, duplicate auth, va notification ownership.

