# JOB-007 Cross-Module Integration Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate Auth, Film, Review, and Notification so the monolith uses one authenticated user context, records film creators, and generates reply notifications.

**Architecture:** Keep all changes inside the current layered architecture by wiring shared contracts in `/Interfaces`, business behavior in `/Services`, and composition in `/Extensions`. Avoid any entity or `Program.cs` changes and use regression tests to lock behavior before implementation.

**Tech Stack:** ASP.NET Core, EF Core InMemory tests, JWT claims via `HttpContextAccessor`, xUnit

---

## Chunk 1: Shared Current User Registration

### Task 1: Add failing test for `CurrentUserService`

**Files:**
- Create: `ReviewFilms.Tests/CurrentUserServiceTests.cs`
- Modify: `ReviewFilms.Tests/ReviewFilms.Tests.csproj`

- [ ] **Step 1: Write the failing test**
- [ ] **Step 2: Run the targeted test to verify it fails**
- [ ] **Step 3: Implement minimal service or DI changes**
- [ ] **Step 4: Run the targeted test to verify it passes**

### Task 2: Remove duplicate current-user registrations

**Files:**
- Modify: `Extensions/ServiceCollectionExtensions.cs`
- Modify: `Extensions/ReviewModuleExtensions.cs`
- Modify: `Extensions/NotificationModuleExtensions.cs`
- Delete: `Security/MockCurrentUserService.cs`

- [ ] **Step 1: Add/adjust coverage for module registration path if needed**
- [ ] **Step 2: Implement single registration of `ICurrentUserService`**
- [ ] **Step 3: Verify all four module extensions are invoked from composition root**

## Chunk 2: Auth -> Film Integration

### Task 3: Add failing test for movie creator assignment

**Files:**
- Create: `ReviewFilms.Tests/MovieServiceTests.cs`
- Modify: `Services/MovieService.cs`

- [ ] **Step 1: Write the failing test asserting `CreatedByUserId` comes from `ICurrentUserService`**
- [ ] **Step 2: Run the targeted test to verify it fails**
- [ ] **Step 3: Inject `ICurrentUserService` and implement minimal assignment**
- [ ] **Step 4: Run the targeted test to verify it passes**

## Chunk 3: Review -> Notification Integration

### Task 4: Add failing tests for reply notifications

**Files:**
- Create: `ReviewFilms.Tests/ReviewServiceTests.cs`
- Modify: `Services/ReviewService.cs`

- [ ] **Step 1: Write failing tests for reply-to-other-user and self-reply flows**
- [ ] **Step 2: Run the targeted tests to verify they fail**
- [ ] **Step 3: Inject `INotificationService` and implement notification creation**
- [ ] **Step 4: Run the targeted tests to verify they pass**

## Chunk 4: Verification

### Task 5: Run project verification

**Files:**
- Review only

- [ ] **Step 1: Run targeted test suites for all new regression tests**
- [ ] **Step 2: Run the full test project**
- [ ] **Step 3: Build the API project if needed**
- [ ] **Step 4: Summarize changed files and resulting runtime flow**
