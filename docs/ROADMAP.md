# BackEnd Web API Product & Engineering Roadmap

---

## 🟢 Phase 1: Clean Architecture Solution Foundation (100% COMPLETE)

Status: **COMPLETED v1.0.0**

### Completed Milestones:
- [x] **Phase 1A — Solution & Project Structure Setup**: Created `.sln` and standard Clean Architecture project layers (`CCTV.Api`, `CCTV.Application`, `CCTV.Domain`, `CCTV.Infrastructure`, `CCTV.Persistence`, `CCTV.Contracts`, `CCTV.Shared`).
- [x] **Phase 1B — API Host & Swagger OpenApi Initialization**: Configured ASP.NET Core Web API host and OpenAPI Swagger UI integration.
- [x] **Phase 1C — Dependency Injection Wiring**: Configured composition root and service collection registrations.

---

## 🔵 Phase 2: Database Persistence & Core API Services (In Progress)

### Target Milestones:
- [ ] **Phase 2A — Entity Framework Core & DB Migrations**: Configure DbContext, PostgreSQL/SQL Server providers, and entity mappings for Cameras, Rooms, Users, and Analytics Events.
- [ ] **Phase 2B — Authentication & Authorization Module**: Implement JWT / Identity endpoints (`/api/auth/login`, `/api/auth/register`, role guards).
- [ ] **Phase 2C — Camera & Spatial Management Endpoints**: CRUD REST endpoints for camera configuration, RTSP stream URL registration, and room zone definitions.
- [ ] **Phase 2D — Analytics Aggregation & Telemetry API**: Aggregation endpoints (`/api/dashboard/stats`) serving real-time telemetry data to FrontEnd ApexCharts.
- [ ] **Phase 2E — Real-time WebSockets & IPC Bridge**: SignalR / WebSocket notification hub and gRPC/MQTT IPC communication bridge to the external Python AI Camera Analytics Service.


