# JOB-010 Code Workflow And Debug Report

## 1. Muc dich tai lieu nay

Tai lieu nay duoc viet de giup ban:

- hieu nhanh JOB-010 da sua gi va vi sao phai sua
- biet request `GET /api/movies` di qua nhung tang nao
- biet `personId` duoc bind, truyen va ap vao query o dau
- biet tai sao filter nay khong tao N+1 query
- biet nen dat breakpoint o dau khi API tra ket qua sai
- biet test nao dang khoa behavior cua JOB-010

Neu sau nay UI co man hinh click vao ho so `Person` va can hien danh sach phim lien quan, day la tai lieu de lan nguoc tu endpoint den query DB.

---

## 2. Bai toan cua JOB-010

Truoc JOB-010, API `GET /api/movies` da ho tro:

- `search`
- `genreId`
- `status`

Nhung chua ho tro:

- `personId`

He qua la UI co the hien danh sach `Person`, nhung khi click vao 1 nguoi cu the thi khong the goi lai cung endpoint de lay cac phim ma nguoi do tham gia.

JOB-010 bo sung kha nang loc nay bang cach:

1. mo rong contract cua `IMovieService`
2. cap nhat `MovieService.GetMoviesAsync`
3. cap nhat `MoviesController.GetMovies`
4. them test bao ve behavior va test pattern SQL

Rang buoc quan trong:

- khong sua `Entities`
- van giu layered architecture
- khong dua business logic vao controller

---

## 3. File da thay doi trong JOB-010

### File production

- `Interfaces/IMovieService.cs`
- `Services/MovieService.cs`
- `Controllers/MoviesController.cs`

### File test

- `ReviewFilms.Tests/MovieServiceTests.cs`
- `ReviewFilms.Tests/MoviesControllerSyncTests.cs`

---

## 4. Thay doi tong quan

### 4.1. O controller

`MoviesController.GetMovies` nhan them:

- `[FromQuery] Guid? personId = null`

Va truyen tham so nay xuong service.

Y nghia:

- controller chi lam nhiem vu nhan input HTTP
- controller khong tu viet query hay thao tac voi EF Core

### 4.2. O service contract

`IMovieService.GetMoviesAsync` nhan them:

- `Guid? personId = null`

Y nghia:

- moi caller cua service deu thay ro service da ho tro filter theo `Person`
- khong co "tham so ngam" hay doc truc tiep tu query string trong service

### 4.3. O service implementation

`MovieService.GetMoviesAsync` duoc bo sung nhanh:

- neu `personId` co gia tri, chi giu lai nhung `Movie` co it nhat 1 dong `MovieCredit` trung `PersonId`

Viec loc nay duoc dat tren `IQueryable<Movie>` truoc:

- `LongCountAsync`
- `OrderBy`
- `Skip`
- `Take`
- `Select`

Dieu nay rat quan trong, vi toan bo filter se di xuong SQL thay vi loc trong memory.

---

## 5. Workflow tu HTTP den DB

## 5.1. Request tu client

Vi du request:

```http
GET /api/movies?pageNumber=1&pageSize=10&personId=0d4f52cc-7c1d-4a4c-a94a-2ea37ef3f3db
```

Client co the gui kem:

- chi `personId`
- hoac ket hop `personId` voi `search`
- hoac ket hop `personId` voi `genreId`
- hoac ket hop ca `search + genreId + status + personId`

## 5.2. Controller layer

`MoviesController.GetMovies` nhan cac query param:

- `pageNumber`
- `pageSize`
- `search`
- `genreId`
- `status`
- `personId`

Sau do controller goi:

```csharp
_movieService.GetMoviesAsync(pageNumber, pageSize, search, genreId, status, personId, cancellationToken)
```

Controller khong can biet `MovieCredits` la bang nao va khong can biet query SQL se ra sao. Day la ranh gioi dung cua layered architecture.

## 5.3. Service layer

Trong `MovieService.GetMoviesAsync`:

1. chuan hoa `pageNumber`
2. clamp `pageSize`
3. tao `query = _dbContext.Movies.AsNoTracking().AsQueryable()`
4. lan luot ghep them filter neu tung tham so co gia tri
5. tinh `totalCount`
6. ap paging
7. project sang `MovieDto`
8. tra `PagedResult<MovieDto>`

Thu tu filter hien tai:

1. `search`
2. `genreId`
3. `status`
4. `personId`

Thu tu nay khong doi business meaning, vi EF Core se hop nhat chung thanh 1 truy van SQL. Dieu quan trong khong phai la thu tu `if`, ma la tat ca `Where(...)` deu dang duoc ghep tren cung mot `IQueryable`.

## 5.4. DB layer

Filter moi duoc viet theo y tuong:

```csharp
movie.MovieCredits.Any(movieCredit => movieCredit.PersonId == personId.Value)
```

Voi EF Core + provider MySQL, pattern nay duoc dich thanh SQL co dang `EXISTS (...)`.

Y nghia:

- khong can `Include(movie => movie.MovieCredits)` de loc
- khong tai toan bo credits len memory
- khong tao vong lap query theo tung movie

Noi ngan gon:

- mot query SQL
- filter bang `EXISTS`
- projection phang sang `MovieDto`

---

## 6. Vi sao filter nay khong gay N+1

N+1 thuong xay ra khi:

1. load danh sach movie
2. voi moi movie lai query tiep credits rieng

JOB-010 khong di theo cach do.

Thay vao do:

- `MovieCredits.Any(...)` nam ngay trong bieu thuc LINQ cua `IQueryable`
- EF Core phan tich expression tree va dich thanh SQL
- danh sach `MovieDto` duoc tao bang `.Select(...)` truc tiep tu query

Vay nen:

- khong co `foreach` movie roi query credit ben trong
- khong co lazy loading
- khong co `Include` du thua cho use case list page

Voi use case danh sach, day la cach dung va gon nhat.

---

## 7. Phan tich chi tiet tung file

## 7.1. `Interfaces/IMovieService.cs`

Day la noi khai bao contract cua service.

Truoc JOB-010, `GetMoviesAsync` chua nhan `personId`.

Sau JOB-010:

- them `Guid? personId = null`

Tac dung:

- bat buoc implementation va caller phai thong nhat chu ky ham
- tranh tinh trang controller ho tro tham so moi nhung service contract chua cap nhat

Day la thay doi nho nhung la diem "neo contract" quan trong nhat cua JOB-010.

## 7.2. `Controllers/MoviesController.cs`

Controller duoc cap nhat them 1 query param:

- `[FromQuery] Guid? personId = null`

Sau do truyen xuong service.

Neu sau nay bug xay ra theo kieu:

- UI gui `personId` nhung service khong nhan duoc

Thi day la file dau tien ban can kiem tra.

Nhung cau hoi can soi:

- ten parameter co dung la `personId` khong
- co gan `[FromQuery]` khong
- thu tu tham so truyen vao service co bi lech khong

## 7.3. `Services/MovieService.cs`

Day la noi chua business logic thuc te cua JOB-010.

Workflow trong method `GetMoviesAsync`:

1. tao `query` tu `_dbContext.Movies`
2. ap `search` neu co
3. ap `genreId` neu co
4. ap `status` neu co
5. ap `personId` neu co
6. dem tong so ban ghi sau khi da loc
7. sort + paging
8. project thanh `MovieDto`

Phan can ghi nho nhat:

- `personId` khong thay doi shape cua DTO
- `personId` chi thay doi tap `Movie` duoc chon

No khong can bo sung field moi vao `MovieDto`, vi use case chi can loc danh sach, khong can tra ve chi tiet person trong response list.

---

## 8. Why `MovieCredits.Any(...)` la lua chon dung

Co 3 cach de lam filter theo person:

1. `Include(MovieCredits)` roi loc trong memory
2. join thu cong giua `Movies` va `MovieCredits`
3. dung navigation + `.Any(...)`

JOB-010 chon cach 3 vi:

- gon
- de doc
- EF Core dich tot
- phu hop voi entity relation da co san

So sanh nhanh:

### 8.1. Loc trong memory

Khong dung vi:

- keo nhieu data len ung dung
- ton bo nho
- de sinh bug paging sai
- de gay N+1 hoac du lieu thua

### 8.2. Join thu cong

Lam duoc, nhung:

- dai dong
- kho doc hon
- khong can thiet cho bai toan don gian nay

### 8.3. `.Any(...)`

Day la diem can bang tot nhat:

- ro nghia nghiep vu
- de debug
- duoc test SQL pattern bang `ToQueryString()`

---

## 9. Cac test dang bao ve JOB-010

## 9.1. `GetMoviesAsync_filters_movies_by_person_id`

Muc tieu:

- bao ve behavior nghiep vu chinh

Test nay seed:

- 2 `Person`
- 2 `Movie`
- 2 `MovieCredit`

Chi 1 movie gan voi `targetPersonId`.

Sau do test goi:

```csharp
service.GetMoviesAsync(1, 10, personId: targetPersonId)
```

Va assert:

- chi co 1 movie duoc tra ve
- dung movie mong doi

Neu sau nay ai sua query sai, test nay se fail rat som.

## 9.2. `Movie_person_filter_query_pattern_translates_to_exists`

Muc tieu:

- khoa ky vong ve hieu nang query

Test nay khong chay query that xuong DB. No chi yeu cau EF Core sinh SQL string bang `ToQueryString()`.

Sau do assert:

- SQL sinh ra phai chua `EXISTS`

Y nghia:

- test nay khong chung minh toàn bo execution plan cua MySQL
- nhung no chung minh pattern LINQ dang duoc dich theo huong mong doi, khong phai loc trong memory

## 9.3. `GetMovies_exposes_person_id_as_query_parameter`

Muc tieu:

- khoa contract cua controller

Test nay dung reflection de kiem tra:

- method `GetMovies` co parameter ten `personId`
- kieu la `Guid?`
- co `[FromQuery]`

Neu sau nay ai doi ten param, xoa `[FromQuery]`, hoac doi kieu sai, test nay se bao dong.

---

## 10. Ban do debug theo trieu chung

## 10.1. Trieu chung: UI gui `personId` nhung API tra ve tat ca phim

Kiem tra theo thu tu:

1. request query string co that su chua `personId` khong
2. `MoviesController.GetMovies` co nhan duoc gia tri khong
3. `MovieService.GetMoviesAsync` co vao nhanh `if (personId.HasValue)` khong
4. `MovieCredits` trong DB co dong nao trung `PersonId` khong

Breakpoint nen dat:

- dau `MoviesController.GetMovies`
- dau `MovieService.GetMoviesAsync`
- ngay trong nhanh `if (personId.HasValue)`

## 10.2. Trieu chung: API tra ve rong du co phim lien quan

Kiem tra:

1. `personId` UI gui co dung GUID cua bang `Persons` khong
2. bang `MovieCredits` co lien ket dung `MovieId - PersonId` khong
3. movie do co bi loai bo boi `status` hay `genreId` hay `search` khong
4. co bi paging cat mat ket qua khong

Can nho:

- `personId` khong phai filter doc lap neu client dong thoi gui them `search`, `genreId`, `status`
- tat ca filter duoc AND voi nhau

Day la nguon gay nham lan rat hay gap khi debug.

## 10.3. Trieu chung: ket qua dem tong (`totalCount`) sai

Kiem tra:

- `LongCountAsync` dang duoc goi truoc hay sau khi ap `personId`

Trong implementation hien tai, `personId` duoc ap truoc `LongCountAsync`, nen `totalCount` va `items` dung cung mot tap filter.

Neu sau nay ai dat filter sau `LongCountAsync`, API se gap bug:

- `items` dung
- nhung tong so trang sai

## 10.4. Trieu chung: nghi ngo co N+1

Kiem tra:

1. query list co them `Include(MovieCredits)` bat thuong khong
2. co doan nao lap qua `items` roi goi DB tiep khong
3. `ToQueryString()` cua pattern `.Any(...)` con ra `EXISTS` khong

Dat breakpoint/inspect:

- ngay truoc `LongCountAsync`
- ngay truoc `ToListAsync`

Neu can xac minh sau hon, bat EF Core SQL logging.

## 10.5. Trieu chung: filter theo person dung trong test in-memory nhung sai tren MySQL

Huong dieu tra:

1. chay lai test `Movie_person_filter_query_pattern_translates_to_exists`
2. xem SQL string sinh ra
3. doi chieu relation `Movie -> MovieCredits`
4. kiem tra data that trong MySQL

Ly do can test SQL pattern rieng:

- test in-memory chi chung minh logic LINQ
- no khong chung minh provider MySQL se dich expression dung nhu mong doi

---

## 11. Diem dat breakpoint de nhin du lieu nhanh

Neu ban muon debug bang Visual Studio, day la 4 diem dat breakpoint hieu qua nhat:

1. `MoviesController.GetMovies`
   Muc tieu: xac minh ASP.NET bind `personId` dung.

2. dau `MovieService.GetMoviesAsync`
   Muc tieu: xem gia tri input sau khi controller truyen xuong.

3. nhanh `if (personId.HasValue)`
   Muc tieu: xac minh request co di vao filter moi hay khong.

4. ngay truoc `var totalCount = await query.LongCountAsync(...)`
   Muc tieu: inspect `query` va neu can thi xem `query.ToQueryString()`.

Neu ban nghi bug nam o DB/data, them breakpoint sau `ToListAsync` de xem `items` da bi cat boi paging hay chua.

---

## 12. Nhung diem hien tai la expected behavior, khong phai bug

- `personId` la optional. Neu khong gui len, API van chay nhu cu.
- `personId` ket hop voi cac filter khac theo logic AND.
- list `MovieDto` van la DTO phang, khong tu dong tra ve credits/person detail.
- service khong can `Include(MovieCredits)` cho use case list page.
- filter theo person chi dua tren bang `MovieCredits`, khong dua tren `KnownForDepartment` hay field nao khac cua `Person`.

Neu business sau nay muon:

- chi loc theo dao dien
- hoac chi loc theo dien vien

Thi JOB-010 chua lam den muc do do. Luc do can them filter tiep theo `CreditType`, `Department`, `Job`.

---

## 13. Goi y khi mo rong sau nay

Neu can nang cap tiep bo loc person, huong mo rong an toan la:

1. giu `personId` nhu hien tai de loc "nguoi nay co lien quan den phim"
2. neu can, them tham so moi nhu:
   - `CreditType? creditType`
   - `string? department`
   - `string? job`
3. tiep tuc ghep tren `IQueryable`
4. viet test behavior truoc
5. them test SQL pattern neu bieu thuc LINQ phuc tap hon

Khong nen:

- tach sang loc trong memory
- them `Include` chi de phuc vu filter
- dua rule query vao controller

---

## 14. Verification da chay cho JOB-010

Da verify bang test project:

```powershell
dotnet test ReviewFilms.Tests\ReviewFilms.Tests.csproj -p:OutputPath=D:\temp\reviewfilms-job010-output\
```

Ket qua:

- pass `23/23` tests

Ly do dung `OutputPath` tam:

- workspace co mot process dang giu file build mac dinh
- doi output path giup test chay on dinh ma khong dung vao process dang mo

Thong tin nay huu ich neu sau nay ban gap lai loi lock file khi verify tren may local.

---

## 15. Ket luan

JOB-010 la mot thay doi nho ve mat code, nhung dung diem ve mat nghiep vu:

- UI co the tim phim theo `Person`
- controller van mong
- service van la noi chua logic query
- EF Core van sinh SQL toi uu theo huong `EXISTS`
- test da khoa ca behavior, contract va pattern SQL

Neu can debug sau nay, hay bat dau theo thu tu:

1. request co gui `personId` khong
2. controller co bind dung khong
3. service co vao nhanh `personId` khong
4. `MovieCredits` trong DB co data dung khong
5. SQL sinh ra co con theo huong `EXISTS` khong

Di dung 5 buoc nay, ban se khoanh vung duoc loi rat nhanh.
