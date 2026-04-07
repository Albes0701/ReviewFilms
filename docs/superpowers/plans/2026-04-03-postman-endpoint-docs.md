# Postman Endpoint Docs Implementation Plan

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tao bo tai lieu Postman theo muc chi tiet tung endpoint de nguoi dung co the sinh request va test script day du tu code hien tai.

**Architecture:** Giu mot file overview o `docs/postman`, mot template chuan, va cac file endpoint-level trong `docs/postman/modules/<domain>`. Moi file phan tach ro request schema, response schema, error mapping, va case matrix.

**Tech Stack:** ASP.NET Core Web API source inspection, Markdown docs, Postman environment/scripts

---

## Chunk 1: Common Documentation Backbone

### Task 1: Cap nhat template Postman

**Files:**
- Modify: `docs/postman/POSTMAN_MODULE_TEMPLATE.md`

- [ ] **Step 1: Dua template ve muc endpoint-level thay vi module-level**
- [ ] **Step 2: Bo sung env vars, error shape, auth, enum, multipart guidance**
- [ ] **Step 3: Them mau case matrix va test script de tai su dung**

### Task 2: Tao file overview

**Files:**
- Create: `docs/postman/POSTMAN_OVERVIEW.md`

- [ ] **Step 1: Liet ke module tree va route-to-file map**
- [ ] **Step 2: Chot env variables, seed prerequisites, va execution order**
- [ ] **Step 3: Ghi ro nuance cua source code de tranh test sai**

## Chunk 2: Auth + Movies Endpoint Docs

### Task 3: Tao auth docs

**Files:**
- Create: `docs/postman/modules/auth/AUTH_REGISTER.md`
- Create: `docs/postman/modules/auth/AUTH_LOGIN.md`
- Create: `docs/postman/modules/auth/AUTH_REFRESH.md`

- [ ] **Step 1: Mo ta request/response/env save cho auth flow**
- [ ] **Step 2: Liet ke validation, auth, rotation, duplicate cases**
- [ ] **Step 3: Them pre-request/test script goi y**

### Task 4: Tao movie docs

**Files:**
- Create: `docs/postman/modules/movies/MOVIES_LIST.md`
- Create: `docs/postman/modules/movies/MOVIES_GET_BY_ID.md`
- Create: `docs/postman/modules/movies/MOVIES_CREATE.md`
- Create: `docs/postman/modules/movies/MOVIES_UPDATE.md`

- [ ] **Step 1: Mo ta query, route, multipart schema, va response paging/detail**
- [ ] **Step 2: Liet ke case filter, slug, genre, auth, not found, boundary**
- [ ] **Step 3: Them huong dan save `movie_id` va cac script assert cot loi**

## Chunk 3: Reviews + Notifications Endpoint Docs

### Task 5: Tao review docs

**Files:**
- Create: `docs/postman/modules/reviews/REVIEWS_RATINGS_UPSERT.md`
- Create: `docs/postman/modules/reviews/REVIEWS_RATINGS_DELETE.md`
- Create: `docs/postman/modules/reviews/REVIEWS_COMMENTS_CREATE.md`
- Create: `docs/postman/modules/reviews/REVIEWS_COMMENTS_LIST_BY_MOVIE.md`

- [ ] **Step 1: Tach ro rating flow va comment flow**
- [ ] **Step 2: Liet ke case auth, range, parent/root/reply, ownership, not found**
- [ ] **Step 3: Chi ro dependency giua movie -> comment -> notification**

### Task 6: Tao notification docs

**Files:**
- Create: `docs/postman/modules/notifications/NOTIFICATIONS_LIST.md`
- Create: `docs/postman/modules/notifications/NOTIFICATIONS_MARK_AS_READ.md`
- Create: `docs/postman/modules/notifications/NOTIFICATIONS_CREATE.md`

- [ ] **Step 1: Mo ta paging, mark-read, va create schema**
- [ ] **Step 2: Liet ke case auth, ownership, invalid data object, paging boundary**
- [ ] **Step 3: Them script luu `notification_id` va assert `isRead/readAt`**

## Chunk 4: Verification

### Task 7: Ra soat tinh nhat quan docs

**Files:**
- Review only

- [ ] **Step 1: Doi chieu route docs voi controllers**
- [ ] **Step 2: Doi chieu validation docs voi DTOs va middleware**
- [ ] **Step 3: Xac nhan file map trong overview khop voi cay thu muc thuc te**

