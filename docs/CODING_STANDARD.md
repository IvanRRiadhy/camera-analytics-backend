# BackEnd Coding Standard & Guidelines (C# .NET 9)

---

## 1. Architecture & Layering Rules

1. **Strict Clean Architecture Layering**: 
   - `CCTV.Api` ➔ depends on `CCTV.Application` & `CCTV.Contracts`.
   - `CCTV.Application` ➔ depends on `CCTV.Domain` & `CCTV.Contracts`.
   - `CCTV.Persistence` & `CCTV.Infrastructure` ➔ depend on `CCTV.Application` & `CCTV.Domain`.
   - `CCTV.Domain` has **zero external dependencies**.
2. **Interface Segregation**: Application services and repositories must communicate via explicit interfaces (`ICameraRepository`, `IAuthService`).

---

## 2. C# Language & Naming Standards

1. **Naming Conventions**:
   - **PascalCase**: Classes, Interfaces (`ICameraService`), Methods, Record Types, Public Properties, Enums.
   - **camelCase**: Method parameters and local variables.
   - **`_camelCase`**: Private fields (e.g. `private readonly ILogger _logger;`).
2. **Asynchronous Programming**:
   - Always use `async` / `await` for I/O bound operations (`ToListAsync()`, `SaveChangesAsync()`).
   - Suffix async method names with `Async` (e.g. `GetCameraByIdAsync`).
   - Pass `CancellationToken` through asynchronous calls where possible.

---

## 3. Standard Development Pattern (Golden Standard)

Untuk menjaga konsistensi codebase dan mencegah over-engineering, seluruh pengembangan fitur backend wajib mengikuti standar alur berikut:

### 3.1 Alur Eksekusi Baku (Data Flow)
```text
[HTTP Request] 
      │
      ▼
[CCTV.Api / Controller]             ➔ Validasi HTTP, permission attribute, return ActionResult<ApiResponse<T>>
      │
      ▼
[CCTV.Application / Service]        ➔ Validasi bisnis, mapping DTO, koordinasi IUnitOfWork
      │
      ▼
[IUnitOfWork / IGenericRepository]  ➔ Akses data EF Core & commit transaksi SaveChangesAsync()
      │
      ▼
[Database / SQL Server]
```

* **Anti-Pattern Terlarang**: Jangan menginjeksi `IUnitOfWork` langsung ke Controller (seperti pada implementasi lama `SpatialController`). Seluruh interaksi data wajib dibungkus di dalam Service Layer.
* **Golden Reference Code**:
  - Controller: [`UsersController.cs`](file:///c:/Migrasi/Projects/Face-Recognition/CctvPro/camera-analytics-backend/CCTV.Analytics/src/CCTV.Api/Controllers/UsersController.cs)
  - Service Interface: [`IUserService.cs`](file:///c:/Migrasi/Projects/Face-Recognition/CctvPro/camera-analytics-backend/CCTV.Analytics/src/CCTV.Application/Identity/Interfaces/IUserService.cs)
  - Service Implementation: [`UserService.cs`](file:///c:/Migrasi/Projects/Face-Recognition/CctvPro/camera-analytics-backend/CCTV.Analytics/src/CCTV.Application/Identity/Services/UserService.cs)

### 3.2 Kebijakan CQRS & MediatR
1. **Tidak Wajib CQRS untuk CRUD**: Jangan membuat `Command` / `Query` MediatR untuk endpoint CRUD standar guna menghindari ledakan berkas boilerplate yang tidak perlu.
2. **Penggunaan MediatR**: MediatR hanya digunakan untuk mempublikasikan **Domain Events** internal dari `CCTVAnalyticsDbContext` setelah transaksi database berhasil di-commit.

### 3.3 Panduan Query Data (Select / Read) & Aturan `IQueryable`
1. **Query Sederhana**: Gunakan method siap pakai dari `IGenericRepository<T>` (`FindAsync`, `FirstOrDefaultAsync`, `GetByIdAsync`).
2. **Query Kompleks (Join / `.Include()` / Projection)**:
   - Gunakan `_unitOfWork.Repository<T>().GetQueryable()` **hanya di dalam Service Layer**.
   - **Aturan Emas `IQueryable`**: Selalu materialize hasil query (`.ToListAsync()`, `.FirstOrDefaultAsync()`) di dalam Service. **DILARANG KERAS** mengembalikan `IQueryable<T>` ke Controller.
3. **Tanpa Transaksi pada Read**: Jangan memanggil `_unitOfWork.SaveChangesAsync()` saat hanya mengeksekusi operasi baca (SELECT).

---

## 4. API Request & Unified Response Envelope (`ApiResponse<T>`)

Mengadopsi pola standar terpadu (*Single Envelope*): seluruh response API (Single, Paged, maupun Error) menggunakan satu struktur pembungkus `ApiResponse<T>`.

### 4.1 Definisi Envelope Baku
```csharp
public class ApiResponse<T>
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("msg")]
    public string Msg { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("meta")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMetadata? Meta { get; set; }
}
```

### 4.2 Tiga Format Response Baku

1. **GET Paged (List dengan Paginasi)**:
   Properti `meta` otomatis muncul saat pagination aktif:
   ```json
   {
     "success": true,
     "msg": "Success",
     "code": 200,
     "data": [
       { "id": "guid", "name": "Server Room A", "code": "RM-01", "floorName": "2nd Floor" }
     ],
     "meta": {
       "page": 1,
       "limit": 20,
       "totalItems": 95,
       "totalPages": 5,
       "hasNextPage": true,
       "hasPreviousPage": false
     }
   }
   ```

2. **GET Single / Detail / Action Mutasi**:
   Properti `meta` bernilai `null` dan otomatis tidak ditampilkan di JSON:
   ```json
   {
     "success": true,
     "msg": "Success",
     "code": 200,
     "data": {
       "id": "guid",
       "name": "Server Room A",
       "code": "RM-01"
     }
   }
   ```

3. **Error Response**:
   Otomatis dibentuk oleh `CustomExceptionMiddleware`:
   ```json
   {
     "success": false,
     "msg": "Room with ID '...' was not found.",
     "code": 404,
     "data": null
   }
   ```

### 4.3 Standar Pesan Terpusat (`ResponseMessages`)
Dilarang menghamburkan *magic string* di Controller, Service, dan Middleware. Semua pesan response sukses maupun pesan error dikumpulkan di [`CCTV.Contracts/Common/ResponseMessages.cs`](file:///c:/Migrasi/Projects/Face-Recognition/CctvPro/camera-analytics-backend/CCTV.Analytics/src/CCTV.Contracts/Common/ResponseMessages.cs):

```csharp
public static class ResponseMessages
{
    public const string Success = "Operation completed successfully.";
    public const string DatabaseError = "A database error occurred. Please contact the administrator.";
    public const string InternalServerError = "An unexpected internal server error occurred.";
    public const string ValidationFailed = "Validation failed.";

    public static class Users
    {
        public const string Created = "User created successfully.";
        public const string Updated = "User updated successfully.";
        public const string Deleted = "User deleted successfully.";
        public const string NotFound = "User was not found.";
        public const string AlreadyExists = "A user with the specified username or email already exists.";
    }
}
```

* **Aturan Baku Pesan Error**:
  1. **Wajib Berupa `const string` Statis (Zero Allocation)**: Menghindari interpolasi string `$""` dinamis agar nol alokasi heap saat runtime dan memudahkan translasi bahasa (i18n) di Frontend.
  2. **Dilarang Menyisipkan ID / Email ke dalam Pesan (OWASP Account Enumeration)**: Menyisipkan email atau username yang bentrok membuka celah tebak akun (*user enumeration*). Mengulang ID juga bersifat redundan karena client sudah mengetahui ID yang dimintanya. Selalu gunakan pesan error generik.

---

## 5. Request Filter & Projection Extension

### 5.1 BaseFilterRequest
Semua parameter query string list/search wajib mewarisi `BaseFilterRequest`:
```csharp
public class BaseFilterRequest
{
    public int Page { get; set; } = 1;
    public int? PageSize { get; set; }
    public int? Limit { get; set; }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool Desc { get; set; } = false;
}

// Filter spesifik per entitas cukup mewarisi BaseFilterRequest:
public class RoomFilterRequest : BaseFilterRequest
{
    public Guid? FloorId { get; set; }
    public bool? IsActive { get; set; }
}
```

### 5.2 Extension Method Projection & Paging (`ToPagedResultAsync`)
Eksploitasi fitur native LINQ `.Select()` EF Core agar database SQL hanya men-select kolom yang dibutuhkan DTO dan otomatis melakukan JOIN tanpa `.Include()` manual:
```csharp
public static async Task<PagedResult<TDto>> ToPagedResultAsync<T, TDto>(
    this IQueryable<T> query,
    BaseFilterRequest filter,
    Expression<Func<T, TDto>> projection,
    CancellationToken ct = default)
{
    var page = filter.Page < 1 ? 1 : filter.Page;
    var limit = (filter.Limit ?? filter.PageSize) is var l && l is < 1 or > 100 ? 20 : l.Value;

    var totalItems = await query.CountAsync(ct);
    var items = await query
        .Skip((page - 1) * limit)
        .Take(limit)
        .Select(projection) // Native EF Core SQL Projection
        .ToListAsync(ct);

    return new PagedResult<TDto>(items, totalItems, page, limit);
}
```

Contoh di Service:
```csharp
var pagedResult = await query.ToPagedResultAsync(filter, r => new RoomDto(
    r.Id,
    r.Name,
    r.Code,
    r.Floor.Name // Otomatis di-JOIN oleh EF Core di SQL level
), ct);

return ApiResponse<List<RoomDto>>.PaginatedResponse(pagedResult);
```

---

## 6. Centralized Error & Exception Handling

### 6.1 Nol Try-Catch di Controller (Zero Try-Catch Policy)
Controller dilarang membungkus action dengan blok `try-catch`. Seluruh error ditangani secara terpusat oleh **`CustomExceptionMiddleware`**.

### 6.2 Custom Domain Exceptions (`CCTV.Domain/Exceptions/CustomExceptions.cs`)
Gunakan exception semantik pada Service:
- `NotFoundException` ➔ Menghasilkan HTTP 404
- `BusinessException` ➔ Menghasilkan HTTP 400
- `UnauthorizedException` ➔ Menghasilkan HTTP 401

Contoh penggunaan di Service:
```csharp
var room = await _unitOfWork.Repository<Room>().GetByIdAsync(id, ct);
if (room == null)
    throw new NotFoundException($"Room with ID '{id}' was not found.");

if (isDuplicateCode)
    throw new BusinessException($"Room code '{req.Code}' already exists.");
```

### 6.3 Pemetaan Exception di `CustomExceptionMiddleware`
Middleware terpusat menangani seluruh exception dan membungkusnya ke `ApiResponse`:
* `NotFoundException` / `KeyNotFoundException` ➔ 404 (`ApiResponse.NotFound`)
* `BusinessException` / `InvalidOperationException` / `ArgumentException` ➔ 400 (`ApiResponse.BadRequest`)
* `UnauthorizedException` ➔ 401 (`ApiResponse.Unauthorized`)
* `UnauthorizedAccessException` ➔ 403 (`ApiResponse.Forbidden`)
* `DbUpdateException` ➔ 400 (Database operation failure)
* `FluentValidation.ValidationException` ➔ 400 (Validation failure dengan daftar field)
* `Unhandled Exception` ➔ 500 (`ApiResponse.InternalError`)

---

## 7. Standar Validasi Input (FluentValidation)

1. **Dilarang Menggunakan `FluentValidationAutoValidation`**:
   - Auto-validation MVC telah *deprecated* karena rawan deadlock pada validasi asinkron (`MustAsync`).
   - Dilarang menggunakan string parsing manual pada `ModelStateDictionary`.
2. **Registrasi Otomatis via Assembly Scanning**:
   - Daftarkan seluruh validator sekaligus di [`CCTV.Application/DependencyInjection.cs`](file:///c:/Migrasi/Projects/Face-Recognition/CctvPro/camera-analytics-backend/CCTV.Analytics/src/CCTV.Application/DependencyInjection.cs):
   ```csharp
   services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
   ```
3. **Injeksi Eksplisit di Constructor (No Deep Abstraction / Zero Magic)**:
   - Hindari abstraksi berlebih seperti `BaseService` dengan `IServiceProvider.GetService` (menghindari *Service Locator pattern* tersembunyi).
   - Suntikkan interface validator langsung secara transparan pada constructor Service:
   ```csharp
   public class UserService : IUserService
   {
       private readonly IUnitOfWork _unitOfWork;
       private readonly IValidator<CreateUserRequestDto> _createValidator;
       private readonly IValidator<UpdateUserRequestDto> _updateValidator;

       public UserService(
           IUnitOfWork unitOfWork,
           IValidator<CreateUserRequestDto> createValidator,
           IValidator<UpdateUserRequestDto> updateValidator)
       {
           _unitOfWork = unitOfWork;
           _createValidator = createValidator;
           _updateValidator = updateValidator;
       }

       public async Task<UserDto> CreateUserAsync(CreateUserRequestDto req, CancellationToken ct)
       {
           await _createValidator.ValidateAndThrowAsync(req, ct);
           // Logika bisnis...
       }
   }
   ```
4. **Format Response Validasi**:
   - Exception `ValidationException` otomatis ditangkap oleh `CustomExceptionMiddleware` dan menghasilkan response 400 terstruktur:
   ```json
   {
     "success": false,
     "msg": "Validation failed",
     "code": 400,
     "data": [
       { "field": "Code", "message": "'Code' must not be empty." },
       { "field": "FloorId", "message": "'FloorId' is required." }
     ]
   }
   ```
