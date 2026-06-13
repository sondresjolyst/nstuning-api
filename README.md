<p align="center">
  <img src="docs/ns-tuning-black-yellow.png" alt="NS Tuning" width="440">
</p>

<p align="center">
  The backend behind nstuning.no — accounts, dyno-run showcase, report storage, and enquiries.
</p>

---

nstuning-api is the API for **NS Tuning** — dyno and performance tuning. It
handles accounts and auth, stores documented dyno runs and their PDF reports,
sends contact enquiries by email, and serves the admin-managed site content for
[nstuning-app](https://github.com/sondresjolyst/nstuning-app).

## What it does

- **Accounts** — registration, login, JWT + refresh tokens, password reset, and
  roles (`Default` / `Admin`).
- **Dyno runs** — public showcase; admins create runs with a PDF report and a
  cover image, stored on an NFS-backed volume and streamed back by the API.
- **Contact** — enquiries emailed to NS Tuning via Brevo.
- **Site content** — admin-managed homepage sections, branding, vehicle catalog
  (brand → model → variant → engine), and settings.

Built as vertical slices (minimal API endpoints + FluentValidation), with
PostgreSQL via EF Core.

---

## For developers

<details>
<summary>Run, configure, and the endpoints</summary>

### Stack

ASP.NET Core 10 · PostgreSQL (EF Core / Npgsql) · ASP.NET Identity + JWT ·
Mapster · Serilog · AspNetCoreRateLimit · Brevo.

### Run locally

```bash
dotnet restore
dotnet ef database update   # needs local Postgres (see appsettings.Development.json)
dotnet run                  # Swagger at /swagger
```

### Promote a user to Admin

Register via `POST /api/auth/register`, then grant the role:

```sql
INSERT INTO "AspNetUserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM "AspNetUsers" u, "AspNetRoles" r
WHERE u."Email" = 'you@example.com' AND r."Name" = 'Admin';
```

### Configuration

In production, secrets come from environment variables:

| Variable                                                                  | What it's for                          |
| ------------------------------------------------------------------------- | -------------------------------------- |
| `ConnectionStrings__DefaultConnection`                                    | PostgreSQL connection string.          |
| `Jwt__Key`, `Jwt__Issuer`                                                 | JWT signing key and issuer.            |
| `BrevoSettings__ApiKey`, `BrevoSettings__SenderEmail`, `BrevoSettings__SenderName` | Brevo email.                  |
| `Storage__ReportsPath`, `Storage__ImagesPath`                             | NFS-backed mounts for reports/images.  |

### API reference

Run the app and browse **Swagger at `/swagger`** for the current endpoints,
schemas, and auth.

### Layout

```
Features/        # one folder per slice (Auth, DynoRuns, Users, Vehicles, …)
Infrastructure/  # endpoint registration, validation filter
Services/        # email, file storage
Models/          # EF Core entities + DbContext
Migrations/      # EF Core migrations
```

</details>
