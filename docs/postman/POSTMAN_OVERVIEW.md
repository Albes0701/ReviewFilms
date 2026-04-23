# ReviewFilms Postman Test Overview

Tai lieu nay la ban do tong quan de tach Postman collection theo **tung endpoint** dua tren source code hien tai cua ReviewFilms API.

## 1. Pham vi business docs

Co 14 endpoint business duoc tach rieng:

- Auth
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
- Movies
  - `GET /api/movies`
  - `GET /api/movies/{id}`
  - `POST /api/movies`
  - `PUT /api/movies/{id}`
- Reviews
  - `POST /api/reviews/ratings`
  - `DELETE /api/reviews/ratings/{movieId}`
  - `POST /api/reviews/comments`
  - `GET /api/reviews/movies/{movieId}/comments`
- Notifications
  - `GET /api/notifications`
  - `PATCH /api/notifications/{id}/read`
  - `POST /api/notifications`

Ngoai pham vi:

- `WeatherForecastController` vi la scaffold sample, khong phai module business

## 2. Thu muc docs de xay Postman

```text
docs/postman/
  POSTMAN_OVERVIEW.md
  POSTMAN_MODULE_TEMPLATE.md
  modules/
    auth/
      AUTH_REGISTER.md
      AUTH_LOGIN.md
      AUTH_REFRESH.md
    movies/
      MOVIES_LIST.md
      MOVIES_GET_BY_ID.md
      MOVIES_CREATE.md
      MOVIES_UPDATE.md
    reviews/
      REVIEWS_RATINGS_UPSERT.md
      REVIEWS_RATINGS_DELETE.md
      REVIEWS_COMMENTS_CREATE.md
      REVIEWS_COMMENTS_LIST_BY_MOVIE.md
    notifications/
      NOTIFICATIONS_LIST.md
      NOTIFICATIONS_MARK_AS_READ.md
      NOTIFICATIONS_CREATE.md
```

## 3. Route -> file map

| Route | File |
| --- | --- |
| `POST /api/auth/register` | `modules/auth/AUTH_REGISTER.md` |
| `POST /api/auth/login` | `modules/auth/AUTH_LOGIN.md` |
| `POST /api/auth/refresh` | `modules/auth/AUTH_REFRESH.md` |
| `GET /api/movies` | `modules/movies/MOVIES_LIST.md` |
| `GET /api/movies/{id}` | `modules/movies/MOVIES_GET_BY_ID.md` |
| `POST /api/movies` | `modules/movies/MOVIES_CREATE.md` |
| `PUT /api/movies/{id}` | `modules/movies/MOVIES_UPDATE.md` |
| `POST /api/reviews/ratings` | `modules/reviews/REVIEWS_RATINGS_UPSERT.md` |
| `DELETE /api/reviews/ratings/{movieId}` | `modules/reviews/REVIEWS_RATINGS_DELETE.md` |
| `POST /api/reviews/comments` | `modules/reviews/REVIEWS_COMMENTS_CREATE.md` |
| `GET /api/reviews/movies/{movieId}/comments` | `modules/reviews/REVIEWS_COMMENTS_LIST_BY_MOVIE.md` |
| `GET /api/notifications` | `modules/notifications/NOTIFICATIONS_LIST.md` |
| `PATCH /api/notifications/{id}/read` | `modules/notifications/NOTIFICATIONS_MARK_AS_READ.md` |
| `POST /api/notifications` | `modules/notifications/NOTIFICATIONS_CREATE.md` |

## 4. Environment variables nen tao truoc

### Core

- `base_url`
- `access_token`
- `refresh_token`
- `current_user_id`
- `current_username`
- `current_email`
- `auth_password`

### Movies

- `movie_id`
- `movie_slug`
- `movie_title`
- `movie_title_updated`
- `genre_id`
- `genre_id_2`

### Reviews

- `comment_id`
- `parent_comment_id`
- `rating_movie_id`
- `comment_content`

### Notifications

- `notification_id`
- `notification_title`
- `notification_message`

## 5. Seed prerequisites

Mot so case yeu cau du lieu he thong ton tai truoc:

- Bang `roles` can co role code `USER` de `register` thanh cong
- Nen co it nhat 1 `genre` hop le de test `movie create/update` voi `genreIds`
- Can co movie ton tai de test rating/comment neu khong tu tao movie moi trong collection
- Can co token hop le tu `register` hoac `login` de test `reviews` va `notifications`

## 6. Quy tac auth dung theo code that

- `AuthController` la `[AllowAnonymous]`
- `ReviewsController` va `NotificationsController` la `[Authorize]`
- `MoviesController` khong co `[Authorize]`, nhung:
  - `GET /api/movies`
  - `GET /api/movies/{id}`
  la public
  - `POST /api/movies`
  - `PUT /api/movies/{id}`
  van can user hop le vi `MovieService` goi `ICurrentUserService`

## 7. Error mapping quan trong

Validation (`400`) den tu `InvalidModelStateResponseFactory`:

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    "Field: message"
  ]
}
```

Business/auth error den tu `GlobalExceptionMiddleware`:

- `ArgumentException` -> `400`
- `UnauthorizedAccessException` -> `401`
- `KeyNotFoundException` -> `404`
- `InvalidOperationException` -> `409`

## 8. Enum notes can luu y khi viet assert

- `NotificationType` request/response: string
- `MovieStatus` response: co the la so `0/1/2`
- `CommentStatus` response: co the la so

Khi viet Postman test, nen assert vao tap gia tri hop le thay vi hard-code sai kieu.

## 9. Suggested Postman collection structure

```text
ReviewFilms
  Auth
    Register
    Login
    Refresh
  Movies
    List
    Get By Id
    Create
    Update
  Reviews
    Ratings - Upsert
    Ratings - Delete
    Comments - Create
    Comments - List By Movie
  Notifications
    List
    Mark As Read
    Create
```

## 10. Thu tu chay de tao env chain

Flow khoi tao co ban:

1. `Auth/Register` hoac `Auth/Login`
2. `Auth/Refresh`
3. `Movies/Create`
4. `Movies/List`
5. `Movies/Get By Id`
6. `Reviews/Ratings - Upsert`
7. `Reviews/Comments - Create`
8. `Reviews/Comments - List By Movie`
9. `Notifications/List`
10. `Notifications/Mark As Read`

Neu can idempotent/negative suite, co the chay tung request doc lap voi data seed rieng.

## 11. End-to-end flows nen co trong collection

### Flow A: Auth lifecycle

- Register user moi
- Login lai bang username
- Refresh token
- Thu refresh bang refresh token cu de xac nhan rotation

### Flow B: Movie lifecycle

- Login
- Create movie moi
- List movies voi `search`
- Get movie by id
- Update movie

### Flow C: Review to notification

- Login user A
- Tao root comment
- Login user B
- Reply vao comment cua user A
- Login lai user A
- List notifications
- Mark as read

### Flow D: Rating lifecycle

- Login
- Upsert rating lan 1
- Upsert rating lan 2 voi score khac
- Delete rating
- Delete lai lan 2 de nhan `404`

## 12. Cach doc tung file endpoint

Moi file endpoint se co:

- `Endpoint Summary`
- `Source Of Truth`
- `Request Definition`
- `Expected Response`
- `Test Case Matrix`
- `Suggested Postman Setup`
- `Coverage Checklist`

Neu can test an toan, uu tien cac case `Verified from code` truoc, sau do moi mo rong sang `Exploratory`.
