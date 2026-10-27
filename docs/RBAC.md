# Dokumentasi Role-Based Access Control (RBAC) & Level Akses

Dokumen ini menjelaskan spesifikasi arsitektur otorisasi, pembagian peran (*roles*), dan matriks hak akses endpoint API pada backend **CCTV Analytics Platform**.

---

## 1. Filosofi & Arsitektur Otorisasi

Sistem menerapkan model **2 Role Inti** yang sederhana, bersih, dan bebas dari *over-engineering*:

```
                           ┌──────────────────────────┐
                           │      JWT Access Token    │
                           │      Claim: "role"       │
                           └────────────┬─────────────┘
                                        │
                    ┌───────────────────┴───────────────────┐
                    ▼                                       ▼
        ┌───────────────────────┐               ┌───────────────────────┐
        │      Role: Admin      │               │     Role: Operator    │
        │  (All Access & Setup) │               │ (Monitoring & Action) │
        └───────────────────────┘               └───────────────────────┘
```

- **Admin (System & Configuration Master):**
  Role eksklusif dengan hak akses penuh (*all-access*). Bertanggung jawab atas pengelolaan akun pengguna, struktur spatial (gedung, lantai, kamera, zona), katalog model AI, dan perintah remote node worker GPU.
- **Operator (Monitoring & Field Duty):**
  Role operasional harian di control room. Memiliki hak pantau (*read-only*) ke seluruh topologi kamera, telemetri live, event analitik, serta hak aksi operasional darurat (seperti menghentikan sementara pipeline AI kamera di lapangan). **Dilarang keras mengubah konfigurasi, menghapus aset, atau mengelola akun pengguna.**

### Implementasi Kode
Menggunakan konstanta terpusat di `CCTV.Domain.Entities.Identity.AppRoles`:
```csharp
public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Operator = "Operator";

    // Kombinasi role operasional (Admin otomatis lolos di mana pun Operator diizinkan)
    public const string Any = $"{Admin},{Operator}";
}
```

---

## 2. Matriks Hak Akses Endpoint API

### A. Autentikasi & Akun Pengguna (`/api/v1/auth`, `/api/v1/users`)

| Method | Endpoint | Deskripsi | Admin | Operator | Publik |
| :---: | :--- | :--- | :---: | :---: | :---: |
| `POST` | `/api/v1/auth/login` | Login user menggunakan Username & Password | ✅ | ✅ | ✅ |
| `GET` | `/api/v1/auth/me` | Melihat profil pengguna yang sedang login | ✅ | ✅ | ❌ |
| `GET` | `/api/v1/users` | Melihat daftar seluruh pengguna tenant | ✅ | ❌ | ❌ |
| `GET` | `/api/v1/users/{id}` | Melihat detail data pengguna spesifik | ✅ | ❌ | ❌ |
| `POST` | `/api/v1/users` | Mendaftarkan pengguna baru & *assign role* | ✅ | ❌ | ❌ |
| `PUT` | `/api/v1/users/{id}` | Memperbarui profil, status aktif, dan role | ✅ | ❌ | ❌ |
| `DELETE`| `/api/v1/users/{id}` | Menghapus pengguna (*soft-delete*) | ✅ | ❌ | ❌ |

---

### B. Spatial & Hirarki Aset Fisik (`/api/v1/spatial`)

Hirarki: `Site` ➔ `Building` ➔ `Floor` ➔ `Room` ➔ `Camera` ➔ `Zone`

| Method | Endpoint | Deskripsi | Admin | Operator |
| :---: | :--- | :--- | :---: | :---: |
| `GET` | `/api/v1/spatial/sites` | Melihat daftar site / cabang | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/spatial/sites/**` | Tambah, ubah, atau hapus site | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/spatial/buildings` | Melihat daftar gedung | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/spatial/buildings/**` | Tambah, ubah, atau hapus gedung | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/spatial/floors` | Melihat daftar lantai per gedung | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/spatial/floors/**` | Tambah, ubah, atau hapus lantai | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/spatial/rooms` | Melihat daftar ruangan per lantai | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/spatial/rooms/**` | Tambah, ubah, atau hapus ruangan | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/spatial/cameras` | Melihat daftar kamera & status stream | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/spatial/cameras/**` | Registrasi kamera, set URL RTSP, hapus kamera | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/spatial/zones` | Melihat koordinat poligon zona deteksi | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/spatial/zones/**` | Buat poligon baru, kalibrasi koordinat zona | ✏️ Write | ❌ 403 |

---

### C. Katalog Modul AI & Pipeline Kamera (`/api/v1/analytics`)

| Method | Endpoint | Deskripsi | Admin | Operator |
| :---: | :--- | :--- | :---: | :---: |
| `GET` | `/api/v1/analytics/modules` | Melihat katalog modul AI (Face, People Counting, dll) | 👁️ Read | 👁️ Read |
| `POST/PUT/DELETE` | `/api/v1/analytics/modules/**` | Tambah/ubah/hapus definisi modul analitik | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/analytics/pipelines` | Melihat daftar pipeline kamera & modul yang aktif | 👁️ Read | 👁️ Read |
| `PUT` | `/api/v1/analytics/pipelines/{id}/modules` | Mengubah modul AI yang dipasang pada kamera | ✏️ Write | ❌ 403 |

---

### D. AI Engine Orchestration & Worker Nodes (`/api/v1/orchestrator`, `/api/v1/orchestration`)

| Method | Endpoint | Deskripsi | Admin | Operator |
| :---: | :--- | :--- | :---: | :---: |
| `GET` | `/api/v1/orchestration/nodes` | Melihat daftar node worker GPU & status resource | 👁️ Read | 👁️ Read |
| `POST` | `/api/v1/orchestration/nodes/{id}/commands` | Remote command node (*restart, calibrate, terminate*) | ✏️ Write | ❌ 403 |
| `GET` | `/api/v1/orchestrator/pipelines` | Monitoring status seluruh pipeline AI yang berjalan | 👁️ Read | 👁️ Read |
| `POST` | `/api/v1/orchestrator/pipelines/assign` | Menugaskan kamera ke worker engine AI spesifik | ✏️ Write | ❌ 403 |
| `POST` | `/api/v1/orchestrator/pipelines/move` | Memindahkan beban pipeline ke engine worker lain | ✏️ Write | ❌ 403 |
| `POST` | `/api/v1/orchestrator/pipelines/{id}/stop` | **Menghentikan analisa kamera di lapangan** | ✅ Action | ✅ Action |
| `GET` | `/api/v1/orchestrator/health/**` | Melihat skor kesehatan dan heartbeat engine fleet | 👁️ Read | 👁️ Read |

---

### E. Monitoring, Live Telemetri, & Insiden (`/api/v1/monitoring`, `/api/v1/runtime`)

| Method | Endpoint | Deskripsi | Admin | Operator | Publik / Engine |
| :---: | :--- | :--- | :---: | :---: | :---: |
| `GET` | `/api/v1/monitoring/overview` | Ringkasan metrik kamera, site, dan counter analitik | 👁️ Read | 👁️ Read | ❌ |
| `GET` | `/api/v1/monitoring/events` | Feed realtime event deteksi analitik | 👁️ Read | 👁️ Read | ❌ |
| `GET` | `/api/v1/monitoring/incidents` | Feed antrian insiden aktif / alarm keamanan | 👁️ Read | 👁️ Read | ❌ |
| `GET` | `/api/v1/monitoring/snapshot/{id}` | Mengambil frame JPEG snapshot live (untuk `<img>` tag) | 👁️ Read | 👁️ Read | ✅ Bebas |
| `POST` | `/api/v1/monitoring/snapshot/{id}` | Upload frame snapshot JPEG dari AI Engine Python | ❌ | ❌ | ✅ Engine |
| `GET` | `/api/v1/runtime/dashboard` | Dashboard agregasi performa engine & plugin | 👁️ Read | 👁️ Read | ❌ |
| `GET` | `/api/v1/runtime/logs/**` | Log runtime engine AI kamera | 👁️ Read | 👁️ Read | ❌ |

---

### F. Internal Worker Service (`/api/v1/engine/runtime`)

| Method | Endpoint | Deskripsi | Sifat Akses |
| :---: | :--- | :--- | :--- |
| `GET` | `/api/v1/engine/runtime/bootstrap` | Digunakan oleh Python AI Engine worker saat inisialisasi boot untuk mengambil daftar kamera & RTSP | `[AllowAnonymous]` *(Internal Service Network)* |

---

## 3. Struktur Token JWT & Evaluasi Otorisasi

Setiap request yang membutuhkan otorisasi wajib menyertakan header:
```http
Authorization: Bearer <access_token>
```

### Contoh Payload JWT Token
```json
{
  "sub": "6481af57-c1ed-43e9-a391-13e916e6ac1c",
  "username": "admin",
  "email": "admin@bionics.ai",
  "fullname": "System Administrator",
  "name": "System Administrator",
  "app_id": "e078ee93-90c4-4b95-9651-7bf82c8a6d33",
  "tenant_id": "e078ee93-90c4-4b95-9651-7bf82c8a6d33",
  "role": "Admin",
  "nbf": 1793092896,
  "exp": 1793179296,
  "iat": 1793092896,
  "iss": "CCTV.Analytics",
  "aud": "CCTV.Clients"
}
```

### Cara Kerja Evaluasi di ASP.NET Core:
1. Middleware `UseAuthentication()` membaca token dan menyusun `ClaimsPrincipal`.
2. Middleware `UseAuthorization()` mencocokkan `ClaimTypes.Role` dengan nilai atribut di controller:
   - Jika controller memiliki `[Authorize(Roles = AppRoles.Admin)]`, hanya user dengan claim `role: "Admin"` yang diizinkan (Operator mendapat HTTP 403 Forbidden).
   - Jika controller memiliki `[Authorize(Roles = AppRoles.Any)]`, user dengan `role: "Admin"` maupun `role: "Operator"` diizinkan.
   - Jika user belum login / token tidak valid, sistem mengembalikan HTTP 401 Unauthorized.

---

## 4. Rencana Skalabilitas (Future Role: Viewer)

Jika di masa depan dibutuhkan role **Viewer** (misalnya untuk auditor, manajemen eksekutif, atau tamu yang hanya boleh melihat statistik tanpa tombol kontrol apa pun):

1. Daftarkan nama role baru di `AppRoles`:
   ```csharp
   public const string Viewer = "Viewer";
   ```
2. Endpoint monitoring cukup diperluas string konstanta-nya:
   ```csharp
   public const string ReadOnlyRoles = $"{Admin},{Operator},{Viewer}";
   ```
3. Endpoint aksi lapangan seperti `stop pipeline` tetap aman terkunci hanya untuk `Admin,Operator` tanpa mengubah struktur arsitektur yang sudah ada.
