# nstuning-api

ASP.NET Core 10 Web API for NS Tuning — auth, dyno-run showcase, PDF report storage, and contact enquiries.

## Stack

- ASP.NET Core 10, PostgreSQL via EF Core (Npgsql)
- ASP.NET Identity + JWT bearer, roles `Default` / `Admin`
- Mapster, Serilog, AspNetCoreRateLimit, Brevo email
- PDF reports stored on an NFS-backed volume (`Storage:ReportsPath`), streamed by the API

## Run locally

```bash
dotnet restore
dotnet ef database update          # requires local Postgres (see appsettings.Development.json)
dotnet run
```

Swagger at `/swagger`.

### Promote a user to Admin

Register via `POST /api/auth/register`, then grant the Admin role, e.g.:

```sql
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."Email" = 'you@example.com' AND r."Name" = 'Admin';
```

## Configuration

Secrets come from environment variables in production:

- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`, `Jwt__Issuer`
- `BrevoSettings__ApiKey`, `BrevoSettings__SenderEmail`, `BrevoSettings__SenderName`
- `Storage__ReportsPath` (e.g. `/data/reports`, an NFS-backed PVC mount)

## Endpoints

- `POST /api/auth/register`, `POST /api/auth/login`, `POST /api/auth/refresh-token`
- `GET /api/dyno-runs`, `GET /api/dyno-runs/{slug}`, `GET /api/dyno-runs/{id}/report` (public)
- `POST/PUT/DELETE /api/dyno-runs` (Admin, multipart with PDF + cover image)
- `POST /api/contact` (public)
- `GET/PUT /api/settings` (Admin)
