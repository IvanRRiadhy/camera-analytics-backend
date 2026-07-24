# BackEnd Web API Developer Guide (C# .NET 9)

---

## 1. Development Prerequisites

To develop and run the **BackEnd Web API** (`BackEnd/CCTV.Analytics`), ensure you have:
- **.NET 9 SDK** installed
- **IDE**: Visual Studio 2022 / VS Code with C# Dev Kit / JetBrains Rider

---

## 2. Building & Running the C# Solution

### Restore & Build:
```bash
dotnet restore BackEnd/CCTV.Analytics/CCTV.Analytics.sln
dotnet build BackEnd/CCTV.Analytics/CCTV.Analytics.sln
```

### Running the API Project:
```bash
dotnet run --project BackEnd/CCTV.Analytics/src/CCTV.Api/CCTV.Api.csproj
```
*(Swagger UI available at `https://localhost:7193/swagger` or `http://localhost:5194/swagger`)*

### Executing Unit & Integration Tests:
```bash
dotnet test BackEnd/CCTV.Analytics/CCTV.Analytics.sln
```

---

## 3. BackEnd Solution Structure (`CCTV.Analytics`)

- **`src/CCTV.Api/`**: REST API Controllers, Request routing, Program.cs Composition Root, Swagger UI, Auth Middleware.
- **`src/CCTV.Application/`**: Application services, CQRS Commands/Queries, Use-case handlers, DTO Mappings.
- **`src/CCTV.Domain/`**: Core Business Entities (Camera, Room, Event), Aggregates, Value Objects, Domain Events.
- **`src/CCTV.Infrastructure/`**: Hardware communication handlers, WebSockets, background workers, and IPC bridge to external AI Engine.
- **`src/CCTV.Persistence/`**: Entity Framework Core DbContext, DB Migrations, Repository implementations.
- **`src/CCTV.Contracts/`**: Data Transfer Objects (DTOs) passed to/from FrontEnd Web Application.
- **`src/CCTV.Shared/`**: Cross-cutting utilities, constants, and helper extensions.


