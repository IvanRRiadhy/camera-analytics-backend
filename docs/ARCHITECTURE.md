# BackEnd Web API Architecture Specification

---

## 1. End-to-End Web System Architecture

```text
               ┌──────────────────────────────────────────────────────────┐
               │              React Frontend / Web Client                 │
               └────────────────────────────┬─────────────────────────────┘
                                            │ REST API (HTTPS) / WebSockets
                                            ▼
┌──────────────────────────────────────────────────────────────────────────────────────────┐
│                      ASP.NET Core Web API BackEnd (C# .NET 9)                            │
│                                                                                          │
│ ┌──────────────────────┐  ┌───────────────────────┐  ┌─────────────────────────────────┐ │
│ │       CCTV.Api       │  │   CCTV.Application    │  │           CCTV.Domain           │ │
│ │ (Endpoints/Swagger)  │─►│  (CQRS / Use Cases)   │─►│ (Entities, Rules, Aggregates)   │ │
│ └──────────────────────┘  └───────────────────────┘  └─────────────────────────────────┘ │
│            │                          │                              │                   │
│            ▼                          ▼                              ▼                   │
│ ┌──────────────────────────────────────────────────────────────────────────────────────┐ │
│ │             CCTV.Persistence (EF Core)  &  CCTV.Infrastructure                       │ │
│ └────────────────────────────────────────┬─────────────────────────────────────────────┘ │
└──────────────────────────────────────────┬───────────────────────────────────────────────┘
                                           │ gRPC / MQTT / WebSockets IPC Bridge
                                           ▼
               ┌──────────────────────────────────────────────────────────┐
               │        Python AI Camera Analytics Engine Service         │
               └──────────────────────────────────────────────────────────┘
```

---

## 2. Core Architectural Pillars (C# .NET 9 Clean Architecture)

1. **`CCTV.Api` Layer**: Direct RESTful API controller layer, Request Routing, Authentication/Authorization Middleware, OpenAPI Swagger generation, and Composition Root for Dependency Injection.
2. **`CCTV.Application` Layer**: Contains business logic, CQRS handling (Commands & Queries), application interfaces, DTO mapping, and service abstractions for camera/room management.
3. **`CCTV.Domain` Layer**: Central enterprise domain models (Camera, Room, User, Event Log) isolated from external frameworks or database dependencies.
4. **`CCTV.Persistence` Layer**: Entity Framework Core implementation, DbContext management, database migrations, and repository patterns.
5. **`CCTV.Infrastructure` Layer**: Hardware streaming integration, WebSocket push notification handlers, background workers, and IPC communication bridge to the AI Analytics Engine.
6. **`CCTV.Contracts` & `CCTV.Shared`**: Data contracts, request/response models for FrontEnd, and cross-cutting utility extensions.