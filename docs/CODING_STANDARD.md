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

## 3. API & Response Standards

1. **DTO Usage**: Do not return domain entities directly from Controller actions. Map domain entities to DTOs in `CCTV.Contracts`.
2. **Standardized Error Handling**: Use Middleware to catch exceptions and format error responses according to standard RFC 7807 `ProblemDetails`.