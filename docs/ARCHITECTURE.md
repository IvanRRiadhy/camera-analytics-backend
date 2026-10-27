# BackEnd Web API Architecture Specification — Platform v2.0.0

**Last Updated**: October 27, 2026

---

## 1. End-to-End Web System Architecture

```text
               ┌──────────────────────────────────────────────────────────┐
               │              React Frontend / Web Client                 │
               └────────────────────────────┬─────────────────────────────┘
                                            │ REST API (HTTP/HTTPS) &
                                            │ SignalR Hub (/hubs/telemetry)
                                            ▼
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                  ASP.NET Core Web API BackEnd (`BackEnd/CCTV.Analytics`)                 │
│                                                                                          │
│ ┌──────────────────────┐  ┌───────────────────────┐  ┌─────────────────────────────────┐ │
│ │       CCTV.Api       │  │   CCTV.Application    │  │           CCTV.Domain           │ │
│ │ (Controllers/Hubs)   │─►│  (CQRS / MediatR /    │─►│ (6-Tier Spatial Hierarchy,      │ │
│ │                      │  │   Storage Interfaces) │  │  Incidents, Dual Evidence)      │ │
│ └──────────────────────┘  └───────────────────────┘  └─────────────────────────────────┘ │
│            │                          │                              │                   │
│            ▼                          ▼                              ▼                   │
│ ┌──────────────────────────────────────────────────────────────────────────────────────┐ │
│ │             CCTV.Persistence (EF Core)  &  CCTV.Infrastructure                       │ │
│ │  - RabbitMQ Background Consumers (`RuntimeTickConsumerHostedService`, etc.)          │ │
│ │  - S3-Compatible Object Storage (`S3CompatibleStorageService` -> SeaweedFS / MinIO)  │ │
│ │  - SignalR Live Telemetry Broadcaster                                                │ │
│ └──────────────────┬─────────────────────────────────┬─────────────────────────────────┘ │
└────────────────────┼─────────────────────────────────┼───────────────────────────────────┘
                     │ AMQP (`cctv.analytics`)         │ S3 REST API / SQL Server / Redis
                     ▼                                 ▼
┌─────────────────────────────────────────┐ ┌──────────────────────────────────────────────┐
│   Python AI Camera Analytics Engine     │ │   SQL Server 2022 / Redis 7 / SeaweedFS S3   │
└─────────────────────────────────────────┘ └──────────────────────────────────────────────┘
```

---

## 2. Core Architectural Pillars (Clean Architecture)

1. **`CCTV.Api` Layer**:
   - RESTful API Controllers (`Cameras`, `Spatial`, `Incidents`, `Analytics`, `Engines`), SignalR Telemetry Hub (`/hubs/telemetry`), OpenAPI/Swagger, and Dependency Injection Composition Root.
2. **`CCTV.Application` Layer**:
   - CQRS Commands & Queries (MediatR), application service interfaces (`IObjectStorageService`, `IEngineRegistrationService`, `IGenericRepository`, `IUnitOfWork`), and validation rules.
3. **`CCTV.Domain` Layer**:
   - 6-Tier Spatial Hierarchy (`Site -> Building -> Floor -> Room -> Camera -> Zone`), `WorkerNode`, `EngineInstance`, `Incident`, and `DetectionEvidence` with Dual Evidence support (`OriginalMediaUrl`, `EnhancedMediaUrl`) and `StorageProvider` enumeration (`Local`, `S3Compatible`, `AzureBlob`).
4. **`CCTV.Persistence` Layer**:
   - Entity Framework Core `ApplicationDbContext`, entity configurations, audit interceptors, repositories, and SQL Server migrations.
5. **`CCTV.Infrastructure` Layer**:
   - RabbitMQ consumers (`EngineRegistrationConsumerHostedService`, `RuntimeTickConsumerHostedService`, `AnalyticsSnapshotConsumerHostedService`), SignalR real-time telemetry broadcasters, and `S3CompatibleStorageService` (native REST S3 upload, presigned URLs, and object deletion).
6. **`CCTV.Contracts` & `CCTV.Shared`**:
   - Strongly typed integration contracts, AMQP message schemas, and shared result primitives.

---

## 3. Reference Documents in `docs/`

- [`BACKEND_TECHNICAL_REFERENCE.md`](./BACKEND_TECHNICAL_REFERENCE.md) — Exhaustive BackEnd feature & layer technical reference.
- [`BLUEPRINT_BACKEND_API.md`](./BLUEPRINT_BACKEND_API.md) — BackEnd API, database schema, and CQRS blueprint.
- [`BLUEPRINT_SYSTEM_INTEGRATION.md`](./BLUEPRINT_SYSTEM_INTEGRATION.md) — Cross-tier AMQP, SignalR, and S3 system integration blueprint.
