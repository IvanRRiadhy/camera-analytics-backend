# Dokumentasi Role-Based Access Control (RBAC) & Fine-Grained Permissions (PBAC)

Dokumen ini menjelaskan spesifikasi arsitektur otorisasi, hak akses granular (*permissions*), pembagian peran (*roles*), dan matriks hak akses endpoint API pada backend **CCTV Analytics Platform**.

---

## 1. Filosofi & Arsitektur Otorisasi

Sistem menerapkan model **Permission-Based Access Control (PBAC)** dengan standar **3 Verb (`read`, `write`, `execute`)**:

```
┌────────────────────────────────────────────────────────┐
│                   JWT Access Token                     │
│  Claim: "role" (Admin, Operator)                       │
│  Claim: "permission" (spatial.read, users.write, dll)  │
└───────────────────────────┬────────────────────────────┘
                            │
               ┌────────────┴────────────┐
               ▼                         ▼
   ┌───────────────────────┐ ┌───────────────────────┐
   │      Role: Admin      │ │     Role: Operator    │
   │    Wildcard: ["*"]    │ │       (6 Perms)       │
   │   (Pure All Access)   │ │                       │
   └───────────────────────┘ └───────────────────────┘
```

- **Roles sebagai Kontainer Hak Akses:**
  User dihubungkan ke Role (`UserRoles`), dan Role memiliki kumpulan hak akses (`RolePermissions`).
- **Wildcard `*` (Pure All-Access untuk Admin):**
  Role `Admin` memiliki hak akses universal `*`. Setiap endpoint baru di backend otomatis lolos untuk Admin tanpa perlu re-konfigurasi hak akses database atau login ulang.
- **Endpoint Terproteksi Permission:**
  Controller tidak mengecek nama role secara kaku, melainkan mengecek hak akses spesifik via atribut `[RequirePermission(AppPermissions.X)]`. Evaluasi otomatis meloloskan pemegang role `Admin` atau pemegang claim `permission: "*"`.
- **Stateless & Cepat:**
  Daftar permission disematkan langsung ke dalam token JWT saat login (`ClaimTypes.Role` & claim `"permission"`), sehingga otorisasi dievaluasi instan di memory oleh ASP.NET Core `IAuthorizationPolicyProvider` tanpa query database berulang.

---

## 2. Standar 3-Verb Permission Catalog

Setiap permission menggunakan konvensi `{modul}.{verb}`:

| Verb | Karakter Risiko | Cakupan Aksi |
| :---: | :---: | :--- |
| **`read`** | Rendah (Passive) | Query data, streaming RTSP, telemetri live, inspeksi log (semua GET) |
| **`write`** | Sedang (Config/Data) | Menambah, mengedit, atau menghapus data konfigurasi aset dan pengguna |
| **`execute`** | Tinggi (Runtime/Mesin) | Perintah kontrol mesin langsung, intervensi hardware GPU, stop pipeline darurat |

### Katalog Lengkap (`AppPermissions`):

```csharp
public static class AppPermissions
{
    public static class Spatial
    {
        public const string Read = "spatial.read";   // Lihat site, gedung, lantai, kamera, zona
        public const string Write = "spatial.write"; // Tambah/edit/hapus aset fisik
    }

    public static class Analytics
    {
        public const string Read = "analytics.read";   // Lihat katalog modul & pipeline aktif
        public const string Write = "analytics.write"; // Pasang/lepas modul analitik ke kamera
    }

    public static class Orchestrator
    {
        public const string Read = "orchestrator.read";       // Monitoring worker node & pipeline
        public const string Write = "orchestrator.write";     // Assign/move pipeline, remote node command
        public const string Execute = "orchestrator.execute"; // Stop pipeline analitik darurat
    }

    public static class Monitoring
    {
        public const string Read = "monitoring.read";   // Live overview, telemetry FPS/GPU, snapshot
        public const string Write = "monitoring.write"; // Acknowledge insiden alarm / catatan
    }

    public static class Users
    {
        public const string Read = "users.read";   // Lihat daftar user & daftar role/permission
        public const string Write = "users.write"; // Buat/edit/hapus user & update permission role
    }
}
```

---

## 3. Pemetaan Default Role (`DbSeeder`)

- **Role `Admin` (Wildcard `*` — Pure All Access):**
  Memiliki permission universal `*` yang otomatis meloloskan seluruh endpoint yang ada saat ini maupun modul yang akan ditambahkan di masa depan.
  Payload JWT claim: `"permission": "*"` dan output login: `"permissions": ["*"]`.

- **Role `Operator` (6 Permissions — Monitoring & Emergency Action):**
  - `spatial.read`
  - `analytics.read`
  - `orchestrator.read`
  - `orchestrator.execute` *(aksi darurat stop pipeline di lapangan)*
  - `monitoring.read`
  - `monitoring.write`

### Akun Bawaan Default (`DbSeeder`):
| Role | Username | Email | Password |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `admin@bionics.ai` | `Admin@123456` |
| **Operator** | `operator` | `operator@bionics.ai` | `Operator@123456` |

---

## 4. Matriks Hak Akses Endpoint API

### A. Autentikasi, Akun Pengguna, & Role Management

| Method | Endpoint | Required Permission / Guard | Admin | Operator | Publik |
| :---: | :--- | :--- | :---: | :---: | :---: |
| `POST` | `/api/v1/auth/login` | `[AllowAnonymous]` | ✅ | ✅ | ✅ |
| `GET` | `/api/v1/auth/me` | Logged In User | ✅ | ✅ | ❌ |
| `GET` | `/api/v1/permissions` | `users.read` | ✅ | ❌ | ❌ |
| `GET` | `/api/v1/roles` | `users.read` | ✅ | ❌ | ❌ |
| `GET` | `/api/v1/roles/{id}` | `users.read` | ✅ | ❌ | ❌ |
| `POST` | `/api/v1/roles` | `users.write` | ✅ | ❌ | ❌ |
| `PUT` | `/api/v1/roles/{id}/permissions` | `users.write` | ✅ | ❌ | ❌ |
| `DELETE`| `/api/v1/roles/{id}` | `users.write` | ✅ | ❌ | ❌ |
| `GET` | `/api/v1/users` | `users.read` | ✅ | ❌ | ❌ |
| `GET` | `/api/v1/users/{id}` | `users.read` | ✅ | ❌ | ❌ |
| `POST` | `/api/v1/users` | `users.write` | ✅ | ❌ | ❌ |
| `PUT` | `/api/v1/users/{id}` | `users.write` | ✅ | ❌ | ❌ |
| `DELETE`| `/api/v1/users/{id}` | `users.write` | ✅ | ❌ | ❌ |

---

### B. Spatial & Hirarki Aset Fisik (`/api/v1/spatial`)

| Method | Endpoint | Required Permission | Admin | Operator |
| :---: | :--- | :--- | :---: | :---: |
| `GET` | `/api/v1/spatial/**` | `spatial.read` | ✅ | ✅ |
| `POST/PUT/DELETE` | `/api/v1/spatial/**` | `spatial.write` | ✅ | ❌ 403 |

---

### C. Katalog Modul AI & Pipeline Kamera (`/api/v1/analytics`)

| Method | Endpoint | Required Permission | Admin | Operator |
| :---: | :--- | :--- | :---: | :---: |
| `GET` | `/api/v1/analytics/**` | `analytics.read` | ✅ | ✅ |
| `POST/PUT/DELETE` | `/api/v1/analytics/**` | `analytics.write` | ✅ | ❌ 403 |

---

### D. AI Engine Orchestration & Worker Nodes

| Method | Endpoint | Required Permission | Admin | Operator |
| :---: | :--- | :--- | :---: | :---: |
| `GET` | `/api/v1/orchestration/**`, `/api/v1/orchestrator/**` | `orchestrator.read` | ✅ | ✅ |
| `POST` | `/api/v1/orchestration/nodes/{id}/commands` | `orchestrator.write` | ✅ | ❌ 403 |
| `POST` | `/api/v1/orchestrator/pipelines/assign` | `orchestrator.write` | ✅ | ❌ 403 |
| `POST` | `/api/v1/orchestrator/pipelines/move` | `orchestrator.write` | ✅ | ❌ 403 |
| `POST` | `/api/v1/orchestrator/pipelines/{id}/stop` | `orchestrator.execute` | ✅ | ✅ |

---

### E. Monitoring & Live Telemetri

| Method | Endpoint | Required Permission / Guard | Admin | Operator | Publik / Engine |
| :---: | :--- | :--- | :---: | :---: | :---: |
| `GET` | `/api/v1/monitoring/overview` | `monitoring.read` | ✅ | ✅ | ❌ |
| `GET` | `/api/v1/monitoring/events` | `monitoring.read` | ✅ | ✅ | ❌ |
| `GET` | `/api/v1/monitoring/incidents` | `monitoring.read` | ✅ | ✅ | ❌ |
| `GET` | `/api/v1/runtime/**` | `monitoring.read` | ✅ | ✅ | ❌ |
| `GET` | `/api/v1/monitoring/snapshot/{id}` | `[AllowAnonymous]` | ✅ | ✅ | ✅ Bebas |
| `POST` | `/api/v1/monitoring/snapshot/{id}` | `[AllowAnonymous]` | ❌ | ❌ | ✅ Engine |
| `GET` | `/api/v1/engine/runtime/bootstrap` | `[AllowAnonymous]` | ❌ | ❌ | ✅ Internal Engine |

---

## 5. Struktur Token JWT

Contoh payload JWT yang dihasilkan saat login:

```json
{
  "sub": "6481af57-c1ed-43e9-a391-13e916e6ac1c",
  "username": "operator1",
  "email": "operator@bionics.ai",
  "fullname": "Control Room Operator",
  "app_id": "e078ee93-90c4-4b95-9651-7bf82c8a6d33",
  "tenant_id": "e078ee93-90c4-4b95-9651-7bf82c8a6d33",
  "role": "Operator",
  "permission": [
    "spatial.read",
    "analytics.read",
    "orchestrator.read",
    "orchestrator.execute",
    "monitoring.read",
    "monitoring.write"
  ],
  "nbf": 1793092896,
  "exp": 1793179296,
  "iss": "CCTV.Analytics",
  "aud": "CCTV.Clients"
}
```
