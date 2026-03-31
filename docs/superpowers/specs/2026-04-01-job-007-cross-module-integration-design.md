# JOB-007 Cross-Module Integration Design

**Goal:** Connect Auth, Film, Review, and Notification into one end-to-end user journey while preserving the current layered architecture and keeping `/Entities` unchanged.

## Scope

- Replace mock current-user resolution with a single shared `ICurrentUserService` implementation backed by `HttpContext.User` JWT claims.
- Capture the authenticated admin user as `CreatedByUserId` when creating movies.
- Trigger a notification when one user replies to another user's comment.
- Ensure module registration loads all four modules in the monolith through extension methods instead of `Program.cs`.

## Architecture

The integration stays inside existing layers:

- `/Interfaces`: shared contracts remain the cross-module boundary.
- `/Services`: business flow wiring happens here through constructor injection.
- `/Extensions`: DI cleanup happens here so composition remains outside controllers and services.

No entity or `ApplicationDbContext` changes are required.

## Design Decisions

### 1. Shared Current User Resolution

- Keep `CurrentUserService` as the only `ICurrentUserService` implementation.
- Remove `MockCurrentUserService`.
- Register `IHttpContextAccessor` and `ICurrentUserService` once in a shared extension path used by the monolith.
- `CurrentUserService` will continue reading `ClaimTypes.NameIdentifier`, then `sub`, then `nameid`.

### 2. Auth -> Film Integration

- Inject `ICurrentUserService` into `MovieService`.
- In `CreateMovieAsync`, resolve the authenticated user ID and assign it to `movie.CreatedByUserId`.
- Existing movie creation flow remains unchanged otherwise.

### 3. Review -> Notification Integration

- Inject `INotificationService` into `ReviewService`.
- When `CreateCommentAsync` receives a `ParentId`, load the parent comment for the same movie.
- If the parent comment belongs to a different user than the replier, create a notification for the parent author.
- Notification payload will store `movieId` and the new reply `commentId` in `DataJson`.
- Use `NotificationType.CommentReply`, title `"Có người vừa trả lời bình luận của bạn"`, and a concise message describing the reply.

### 4. DI Cleanup

- Ensure `AddApplicationDbContext` calls:
  - `AddAuthModule`
  - `AddFilmModule`
  - `AddReviewModule`
  - `AddNotificationModule`
- Remove duplicate `ICurrentUserService` registrations from individual module extensions.

## Testing Strategy

- Add regression tests for `CurrentUserService` claim parsing and auth failure behavior.
- Add regression tests for `MovieService.CreateMovieAsync` assigning `CreatedByUserId`.
- Add regression tests for `ReviewService.CreateCommentAsync`:
  - reply to another user creates a notification
  - reply to own comment does not create a notification

## Risks

- Duplicate DI registrations could silently override each other if cleanup is incomplete.
- Reply notification must not fire for self-replies.
- The reply payload must remain valid JSON because `Notification.DataJson` maps to a MySQL `json` column.
