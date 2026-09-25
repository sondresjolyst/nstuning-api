<p align="center">
  <img src="docs/ns-tuning-black-yellow.png" alt="NS Tuning" width="440">
</p>

# nstuning-api

API for NS Tuning, serving
[nstuning-app](https://github.com/sondresjolyst/nstuning-app).

## Stack

ASP.NET Core 10, PostgreSQL through EF Core and Npgsql, ASP.NET Identity with
JWT, Mapster, Serilog, AspNetCoreRateLimit, Brevo.

## Quick start

```bash
dotnet restore
dotnet ef database update   # needs a local Postgres, see appsettings.Development.json
dotnet run                  # Swagger at /swagger
```

Migrations do not run at startup. Apply them yourself before a deploy that adds
any.

## Environment

Production reads these from the cluster secret.

| Variable | Used for |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string |
| `Jwt__Key`, `Jwt__Issuer` | JWT signing key and issuer. The key must match the app's `NSTUNING_API_JWT_SECRET` |
| `BrevoSettings__ApiKey`, `BrevoSettings__SenderEmail`, `BrevoSettings__SenderName` | Transactional email |
| `Storage__ReportsPath` | Mount for dyno reports, `/data/reports` in the cluster |
| `Storage__ImagesPath` | Mount for uploaded images, `/data/images` in the cluster |

## What it serves

| Area | Holds |
| --- | --- |
| Dyno runs | Documented runs with figures and a downloadable PDF report |
| Vehicle catalog | Brand, model, variant and engine, reused across runs |
| Content | Home page sections and legal pages |
| Contact | Enquiries emailed through Brevo |
| Accounts | Sign-in, JWT and refresh tokens, password reset, roles |

Browse `/swagger` on a running instance for the current surface.

## Health

| Path | Reports |
| --- | --- |
| `/health` | The process is up. No dependency checks, so a database outage does not restart the pod |
| `/health/ready` | The database connection. Fails while Postgres is unreachable, which takes the pod out of its Service |

Both are anonymous, and both are blocked at the ingress: only the kubelet
reaches them, over the pod address.

## Deployment

Image [`sondresjo/nstuning-api`](https://hub.docker.com/r/sondresjo/nstuning-api)
on Docker Hub, chart `nstuning-api` in
[tumogroup-charts](https://github.com/sondresjolyst/tumogroup-charts), applied by
Flux from [tumo-flux](https://github.com/sondresjolyst/tumo-flux) to
`nstuning-dev` and `nstuning-prod`.

The container runs as the non-root `app` user with a read-only root filesystem,
so anything written at runtime needs a volume. Reports and images go to the
`/data` mounts, and the data protection key ring to `/home/app/.aspnet`.

A push to `main` builds the `dev` tag. A release-please release builds `vX.Y.Z`,
tags it `latest` and opens a chart bump against
[tumogroup-charts](https://github.com/sondresjolyst/tumogroup-charts). Cluster
secrets are created by
[`scripts/nstuning/bootstrap.sh`](https://github.com/sondresjolyst/tumo-platform/blob/main/scripts/nstuning/bootstrap.sh)
in [tumo-platform](https://github.com/sondresjolyst/tumo-platform).

## License

Proprietary. Copyright (c) 2026 Sondre Sjølyst.
