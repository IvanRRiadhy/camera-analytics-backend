# BackEnd Web API Developer Guide — Platform v2.0.0

**Last Updated**: October 27, 2026

---

## 1. Development Prerequisites

To develop and run the **BackEnd Web API** (`BackEnd/CCTV.Analytics`), ensure you have:
- **.NET 8 / .NET 9 SDK** installed
- **Docker Desktop** (for SQL Server 2022, RabbitMQ 3.13, Redis 7, and SeaweedFS S3 Object Storage)
- **IDE**: Visual Studio 2022 / VS Code with C# Dev Kit / JetBrains Rider

---

## 2. Starting Infrastructure Services

From the workspace root (`e:\CCTV Analytics`), start the supporting infrastructure containers:

```powershell
docker compose up -d
```

- **SQL Server**: `localhost:1433`
- **RabbitMQ AMQP / Management UI**: `localhost:5672` / `http://localhost:15672` (`cctv_vhost`)
- **Redis**: `localhost:6379`
- **SeaweedFS S3 API / Filer**: `http://localhost:8333` / `http://localhost:8888`

---

## 3. Building & Running the C# Solution

### Restore & Build:
```powershell
dotnet restore BackEnd/CCTV.Analytics/CCTV.Analytics.sln
dotnet build BackEnd/CCTV.Analytics/CCTV.Analytics.sln
```

### Running the API Project:
```powershell
dotnet run --project BackEnd/CCTV.Analytics/src/CCTV.Api/CCTV.Api.csproj
```
*(API & Swagger UI available at `http://localhost:5100/swagger` or `https://localhost:7193/swagger`)*

### Executing Unit & Integration Tests:
```powershell
dotnet test BackEnd/CCTV.Analytics/CCTV.Analytics.sln
```

---

## 4. BackEnd Solution Structure (`CCTV.Analytics`)

- **`src/CCTV.Api/`**: REST API Controllers, SignalR Hubs (`/hubs/telemetry`), `Program.cs` Composition Root, Swagger UI, Middleware.
- **`src/CCTV.Application/`**: CQRS Commands/Queries, Use-case handlers, `Abstractions/Storage/IObjectStorageService.cs`, DTO Mappings.
- **`src/CCTV.Domain/`**: Core Business Entities (`Site`, `Building`, `Floor`, `Room`, `Camera`, `Zone`, `Incident`, `DetectionEvidence`), Enumerations (`StorageProvider`), Value Objects.
- **`src/CCTV.Infrastructure/`**: RabbitMQ Consumers (`RuntimeTickConsumerHostedService`, `EngineRegistrationConsumerHostedService`), `Storage/S3CompatibleStorageService.cs`, SignalR Notifiers.
- **`src/CCTV.Persistence/`**: Entity Framework Core `ApplicationDbContext`, Configurations, Interceptors, Repositories, and `UnitOfWork`.
- **`src/CCTV.Contracts/`**: Integration Message Contracts and DTOs shared between AI Engine, BackEnd, and FrontEnd.
- **`src/CCTV.Shared/`**: Cross-cutting utilities, constants, and result extensions.
