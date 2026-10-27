# ENTERPRISE CCTV ANALYTICS PLATFORM
## Master System Integration & Architecture Blueprint

**Dokumen**: Master Architecture Blueprint & Cross-Tier Integration Specification  
**Versi**: 2.0.0 Enterprise Edition  
**Tanggal**: Oktober 2026  
**Klasifikasi**: Arsitektur Sistem & Spesifikasi Teknis  
**Status**: Approved Living Architecture  
**Lokasi Berkas**: docs/BLUEPRINT_SYSTEM_INTEGRATION.md  

---

## 1. Executive Summary & Visi Strategis Sistem

### 1.1 Visi Sistem
**Enterprise CCTV Analytics Platform** adalah solusi terdistribusi berkinerja tinggi untuk analitik video cerdas (*intelligent video analytics*), pemantauan spasial multi-kamera, dan manajemen insiden keamanan otomatis secara *real-time*. Platform ini menjembatani simpul inferensi *deep learning* berbasis akselerasi perangkat keras GPU dengan arsitektur backend enterprise .NET 8 (Clean Architecture & CQRS) serta dashboard web interaktif React 18/19 berstandar industri.

### 1.2 Sasaran Kinerja & SLA Sistem
1. **SLA Peringatan Real-Time**: Deteksi anomali, pelanggaran zona perimeter (*intrusion*), pergerakan garis batas (*tripwire*), dan loitering diproses serta disiarkan ke dashboard operator dalam waktu **< 500 ms** sejak frame video diambil di kamera.
2. **Skalabilitas Multi-Engine Horisontal**: Mendukung penambahan node pekerja AI (*Worker Node*) secara elastis tanpa perlu melakukan *restart* pada server aplikasi backend pusat.
3. **Sinkronisasi Status Frekuensi Tinggi**: Penyiaran metrik telemetri kesehatan engine, FPS, dan beban perangkat keras secara kontinu dengan frekuensi **1 Hz** melalui saluran streaming WebSocket (SignalR).
4. **Presisi Spasial Normalisasi**: Seluruh koordinat geometri poligon zona analitik disimpan dalam format float ternormalisasi [0.0, 1.0], menjamin independensi total terhadap resolusi video sumber (720p, 1080p, 4K) maupun ukuran kanvas rendering pada browser client.

---

## 2. Prinsip Rekayasa Arsitektur Enterprise

Platform ini dirancang dengan prinsip-prinsip arsitektur mutlak:
* **Headless & API-First AI Engine**: AI Analytics Engine beroperasi secara mandiri (*headless*). Mesin inferensi tidak boleh terikat dengan framework UI, session browser, ataupun vendor kamera tertentu.
* **Decoupled Clean Architecture**: Logika bisnis inti di Backend (CCTV.Domain) bersih dari ketergantungan framework pihak ketiga, database ORM, maupun transport layer.
* **Dual-Topology Messaging**: Pemisahan jalur data berlatensi rendah untuk streaming metrik (SignalR WebSockets) dengan jalur transmisi pesan kritis berbasis jaminan pengiriman (*guaranteed delivery*) menggunakan AMQP Message Broker (RabbitMQ).
* **Dekompilasi Spasial 6-Tingkat (*Strict Spatial Hierarchy*)**: Pemodelan fisik hierarki spasial menyeluruh untuk memetakan penempatan kamera dan zona analitik pada area operasional secara presisi.

---

## 3. Matriks Tumpukan Teknologi Terpadu (*Unified Tech Stack*)

| Lapisan Arsitektur | Teknologi / Framework Utama | Dependensi & Pustaka Inti | Peran & Tanggung Jawab Operasional |
| :--- | :--- | :--- | :--- |
| **Tier 1: AI Analytics Engine** | Python 3.10+ | OpenCV, PyTorch, CUDA, Ultralytics YOLOv8/Pose, ByteTrack, Pydantic, pika | Pengambilan stream RTSP, inferensi GPU, estimasi pose, tracking multi-objek, evaluasi zona spasial, penerbitan event AMQP. |
| **Tier 2: Backend Control Plane** | C# / .NET 8 Web API | Clean Architecture, MediatR (CQRS), EF Core 8, FluentValidation, Quartz.NET | Orkestrasi logika domain, persistensi basis data, manajemen hierarki spasial, background consumer, SignalR WebSocket Hub. |
| **Tier 3: Frontend Web Dashboard** | React 18 / 19, TypeScript, Vite | Material-UI (MUI) v5, TailwindCSS, TanStack Query v5, Zustand, SignalR Client, Canvas 2D | Visualisasi pemantauan real-time, pohon navigasi spasial, editor kanvas poligon interaktif, suite observabilitas DevTools. |
| **Message Broker (AMQP)** | RabbitMQ 3.13+ | Protokol AMQP 0-9-1, Topic Exchange, Consumer Prefetch | Antrean pesan asinkron, perutean event analitik, pelaporan pendaftaran engine, dan telemetri snapshot. |
| **Real-Time Gateway** | ASP.NET Core SignalR | Protokol WebSockets, Hub Endpoint (/hubs/telemetry) | Penyiaran event berkecepatan tinggi ke ratusan klien web browser dengan latensi sub-50ms. |
| **Primary Database** | SQL Server 2022 / PostgreSQL | EF Core Provider, PostGIS / JSONB Type Mapping | Penyimpanan transaksional aset spasial, konfigurasi kamera, log audit, dan arsip riwayat event insiden. |
| **Cache & Distributed Lock** | Redis 7+ Alpine | Redis In-Memory Key-Value & Queue | Caching status liveness node, session temporary storage, dan peredam lonjakan event. |

---

## 4. Topologi Arsitektur Sistem Menyeluruh

`mermaid
flowchart TB
    subgraph EdgeLayer [Edge Layer: Sumber Video & Kamera]
        RTSP_Cam1[Kamera IP Outdoor RTSP H.264/H.265]
        RTSP_Cam2[Kamera Indoor Ruang Server RTSP]
        NVR_Source[NVR / DVR Multi-Channel Feed]
    end

    subgraph Tier1 [Tier 1: Python AI Analytics Engine Cluster]
        direction TB
        ReaderPool[VideoReader Ingestion Worker Pool]
        RingBuffer[Circular FrameBuffer Deque Capped]
        InferenceEngine[GPU Worker YOLOv8 + Pose + ByteTrack]
        ZoneMath[ZoneEngine Ray-Casting PIP]
        AMQPPub[RabbitMQ Publisher pika]
        
        ReaderPool --> RingBuffer --> InferenceEngine --> ZoneMath --> AMQPPub
    end

    subgraph MessagingPlane [Messaging Plane: Message Broker & Cache]
        RabbitMQBroker[(RabbitMQ Broker cctv.analytics.events)]
        RedisCache[(Redis Cache & Liveness)]
    end

    subgraph Tier2 [Tier 2: ASP.NET Core .NET 8 Enterprise Backend]
        direction TB
        AMQPConsumer[Background Hosted Services RabbitMQ Consumer]
        MediatRBus[MediatR CQRS Pipeline & Handlers]
        DbContext[EF Core 8 Unit of Work]
        TelemetryHub[SignalR Realtime Hub /hubs/telemetry]
        RESTControllers[REST Controllers /api/v1/spatial, /api/v1/engine]
        
        AMQPConsumer --> MediatRBus --> DbContext
        MediatRBus --> TelemetryHub
        RESTControllers --> MediatRBus
    end

    subgraph StoragePlane [Data Storage Plane]
        SQLDatabase[(SQL Server 2022 / PostgreSQL Enterprise)]
    end

    subgraph Tier3 [Tier 3: React Web Dashboard Browser]
        direction TB
        SignalRClient[SignalR WebSocket Client]
        RealtimeDispatcher[RealtimeDispatcher ADR-011]
        DomainStores[Realtime Domain Stores Runtime, Event, Camera, Zone]
        DevToolsSuite[DevTools Observability Center /_devtools]
        DashboardUI[Enterprise Monitoring Operations Center]
        
        SignalRClient --> RealtimeDispatcher --> DomainStores --> DashboardUI
        RealtimeDispatcher --> DevToolsSuite
    end

    EdgeLayer -->|RTSP TCP/UDP Stream| ReaderPool
    AMQPPub -->|AMQP Publish Event & Heartbeat| RabbitMQBroker
    RabbitMQBroker -->|AMQP Consume| AMQPConsumer
    DbContext <-->|Transaksional SQL / ACID| SQLDatabase
    RESTControllers <-->|HTTP REST CRUD| DashboardUI
    TelemetryHub -->|WebSocket Push JSON Stream| SignalRClient
    RESTControllers <-->|Liveness / Status Caching| RedisCache
`

---

## 5. Model Spasial Enterprise: 6-Tier Strict Spatial Hierarchy

Platform mengorganisasikan seluruh aset fisik dan analitik ke dalam hierarki kontainer spasial 6 tingkat yang ketat:

`
[ Site ] (Kampus Fisik / Lokasi Geografis)
   └── [ Building ] (Gedung / Struktur Bangunan Fisik)
        └── [ Floor ] (Lantai Bangunan / Gambar Floorplan 2D)
             └── [ Room / Area ] (Ruangan Kerja / Kontainer Operasional Logis)
                  └── [ Camera ] (Aset Kamera Pengawas / Stream RTSP Ingestion)
                       └── [ Zone ] (Geometri Poligon Spasial [0.0, 1.0])
`

### 5.1 Karakteristik Entitas Spasial
1. **Site (CCTV.Domain.Entities.Spatial.Site)**: Batas teratas kepemilikan aset geografis (misalnya: *Headquarters Jakarta*, *Pabrik Karawang*). Terikat dengan ApplicationId multi-tenant.
2. **Building (Building)**: Bangunan individual di bawah Site (misalnya: *Gedung Menara A*, *Gudang Logistik*).
3. **Floor (Floor)**: Lantai vertikal dalam gedung. Menyimpan referensi URL denah lantai (FloorPlanUrl) dan indeks level (FloorLevel).
4. **Room / Area (Room)**: Kontainer utama operasi spasial indoor (misalnya: *Ruang Server*, *Lobi Utama*, *Area Kasir*). Digunakan untuk analitik kapasitas ruangan (*room occupancy*), antrean, dan pelaporan per area.
5. **Camera (Camera)**: Endpoint RTSP video streaming. Memiliki relasi opsional RoomId? agar kamera outdoor atau kamera jalanan tetap dapat berfungsi tanpa harus berada di dalam ruangan tertutup.
6. **Zone (Zone)**: Poligon 2D yang digambar di atas bidang pandang kamera (PolygonCoordinatesJson). Menyimpan titik-titik (x, y) ternormalisasi [0.0, 1.0] serta tipe aturan (INTRUSION, TRIPWIRE, LOITERING).

---

## 6. Siklus Hidup Alur Data End-to-End (*Data Lifecycles*)

### 6.1 Siklus Hidup Bootstrap & Handshake Engine
Saat AI Engine dinyalakan, proses inisialisasi berlangsung secara otomatis melalui protokol pertukaran status:

`mermaid
sequenceDiagram
    autonumber
    participant Engine as Python AI Engine (main.py)
    participant API as Backend (EngineBootstrapController)
    participant DB as SQL Server / PostgreSQL
    participant MQ as RabbitMQ Broker
    participant Web as Web Dashboard (SignalR)

    Engine->>API: HTTP GET /api/v1/engine/runtime/bootstrap
    Note over Engine,API: Mengirim identitas hardware node & GPU capability
    API->>DB: Query EngineInstance, Penugasan Kamera & Koordinat Zona Aktif
    DB-->>API: Kembalikan Konfigurasi Pipeline & Metadata Spasial
    API-->>Engine: HTTP 200 OK (Konfigurasi JSON Lengkap)
    Engine->>Engine: Inisialisasi Thread VideoReader & Alokasi VRAM Model YOLO
    Engine->>MQ: Publish EngineRegisteredEvent (RoutingKey: cctv.engine.registered)
    MQ->>API: Background Service Menerima Notifikasi Registrasi
    API->>Web: Broadcast Status Engine ONLINE via SignalR Telemetry Hub
`

### 6.2 Siklus Hidup Deteksi Analitik, Eskalasi & Alerting Real-Time
Alur sejak objek tertangkap lensa kamera hingga operator menerima peringatan visual:

`mermaid
sequenceDiagram
    autonumber
    participant Cam as IP Camera
    participant Eng as Python Engine Worker
    participant MQ as RabbitMQ (cctv.analytics.events)
    participant Host as .NET Background Consumer
    participant DB as Database (EventArchive)
    participant Hub as SignalR Telemetry Hub
    participant UI as React Web Dashboard (ADR-011)

    Cam->>Eng: Transmisi Frame RTSP (H.264 @ 25 FPS)
    Eng->>Eng: Deteksi Objek YOLOv8 (BBox Person, Conf: 0.92)
    Eng->>Eng: ByteTrack update(Track ID #104)
    Eng->>Eng: ZoneEngine Evaluasi Titik Kaki BBox vs Poligon Terlarang
    Note over Eng: Titik (x, y) berada di dalam Poligon -> Event INTRUSION
    Eng->>MQ: Publish AnalyticsEvent JSON Payload
    MQ->>Host: Consume Message Asinkron
    par Persistensi Arsip
        Host->>DB: Simpan EventArchive & Cek Rule Eskalasi Insiden
    and Diseminasi Real-Time
        Host->>Hub: Trigger Broadcast ke Grup Klien Terkoneksi
        Hub->>UI: WebSocket Packet: AnalyticsEventPublished
    end
    UI->>UI: RealtimeDispatcher meneruskan ke AnalyticsEventStore
    UI->>UI: Canvas Zone Menyala Merah & Membunyikan Audio Alert
`

---

## 7. Arsitektur Komunikasi Pesan Ganda (*Dual Messaging Topology*)

Platform sengaja membagi lalu lintas komunikasi menjadi dua saluran terpisah guna menghindari bottleneck:

`
                           +---> [RabbitMQ (AMQP)] ---> [Hosted Services] ---> [DB Persistence]
                           |        (Event Kritis & Transaksional)
[Python AI Engine Cluster] |
                           |
                           +---> [SignalR WebSocket] ---> [React Browser Dashboard]
                                    (Telemetri Metrik 1Hz & Live Alerting)
`

1. **Jalur AMQP (RabbitMQ)**:
   * **Tujuan**: Menjamin bahwa setiap event keamanan, pelanggaran, dan pendaftaran node tidak akan hilang (*guaranteed delivery*) meskipun server backend sedang mengalami beban puncak atau restart sesaat.
   * **Exchanges**:
     * cctv.analytics.events (Topic Exchange untuk event deteksi dan snapshot analitik).
     * cctv.engine.lifecycle (Direct Exchange untuk registrasi, heartbeat, dan status node).
2. **Jalur SignalR WebSockets (/hubs/telemetry)**:
   * **Tujuan**: Menghantarkan paket berlatensi ultra-rendah (< 30 ms) langsung ke browser operator tanpa polling HTTP.
   * **Paket yang Ditransmisikan**: Heartbeat FPS kamera, pemanfaatan memori GPU, koordinat tracking sementara (*ephemeral coordinates*), dan notifikasi insiden aktif.

---

## 8. Infrastruktur Kontainer & Profil Deployment (docker-compose.yml)

Platform menyediakan profil kontainer terpadu untuk lingkungan produksi maupun pengujian lokal:

| Layanan Kontainer | Port Host | Port Kontainer | Image / Dasar | Peran Infrastruktur |
| :--- | :--- | :--- | :--- | :--- |
| cctv_sqlserver | 1433 | 1433 | mcr.microsoft.com/mssql/server:2022-latest | Basis data transaksional utama untuk penyimpanan entitas bisnis dan arsip event. |
| cctv_rabbitmq | 5672, 15672 | 5672, 15672 | 
abbitmq:3.13-management-alpine | Broker pesan AMQP 0-9-1 dan dashboard pemantauan antrean web. |
| cctv_redis | 6379 | 6379 | 
edis:7-alpine | Cache status liveness komponen dan distributed lock engine. |
| cctv_backend | 5100 | 8080 | Build dari BackEnd/Dockerfile | ASP.NET Core 8 Web API host dan SignalR hub gateway. |
| cctv_frontend | 3000 | 80 | Build dari FrontEnd/Dockerfile | Nginx web server menyajikan build aset produksi React SPA. |
| cctv_ai_engine | - | - | Host GPU / Container NVIDIA CUDA 12 | Node pekerja inferensi AI memproses stream kamera dan menerbitkan event. |

---

## 9. Keamanan, Autentikasi & Tata Kelola Multi-Tenancy

1. **Autentikasi Berbasis JWT Bearer**:
   * Setiap permintaan REST API mewajibkan token JWT pada header Authorization: Bearer <token>.
   * Koneksi WebSocket SignalR melekatkan token melalui query string parameter ?access_token=<token> yang diekstrak pada tahap negosiasi handshake HTTP.
2. **Isolasi Multi-Tenancy (ApplicationId)**:
   * Penyedia tenant backend (HttpContextTenantProvider) mengekstrak klaim pp_id atau 	enant_id dari token JWT.
   * Seluruh kueri basis data pada entitas turunan TenantEntity (seperti Site, Camera, Zone) secara otomatis difilter berdasarkan ApplicationId pengguna aktif untuk mencegah kebocoran data antar organisasi.
3. **Role-Based Access Control (RBAC)**:
   * Level akses dibedakan menjadi: SuperAdmin, SecurityOperator, Auditor, dan IntegrationClient.
   * Rute /_devtools dan kontrol konfigurasi node mesin hanya dapat diakses oleh pengguna dengan wewenang administratif.

---

## 10. Dokumen Terkait & Referensi Silang
* [Spesifikasi Teknis AI Engine](file:///e:/CCTV%20Analytics/docs/BLUEPRINT_AI_ENGINE.md)
* [Spesifikasi Teknis Backend Control Plane](file:///e:/CCTV%20Analytics/docs/BLUEPRINT_BACKEND_API.md)
* [Spesifikasi Teknis Frontend Dashboard](file:///e:/CCTV%20Analytics/docs/BLUEPRINT_FRONTEND_DASHBOARD.md)
