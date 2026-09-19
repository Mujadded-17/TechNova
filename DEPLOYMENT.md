# Deploying Tech Nova to Render

The app ships as a Docker container. Render builds the `Dockerfile`, mounts a
persistent disk for uploads, and reads every secret from environment variables.

Render has **no managed SQL Server**, so the database is hosted elsewhere
(Azure SQL or any reachable SQL Server) and the app connects out to it.

---

## 1. Provision the database

Create a SQL Server database with your provider and collect its connection
string. It must be reachable from Render, so allow public network access and,
where the provider supports an allow-list, permit Render's outbound IPs (listed
under **Connect > Outbound** on the service once created).

Always require encryption:

```
Server=tcp:YOUR-SERVER.database.windows.net,1433;Initial Catalog=TechNovaDb;User ID=…;Password=…;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

Schema is created automatically on first boot — `Database__MigrateOnStartup` is
set to `true` in `render.yaml`, so the nine EF migrations apply during startup.

## 2. Create the service

Push the repository to GitHub, then in Render choose **New > Blueprint** and
point it at the repo. `render.yaml` defines:

- a Docker web service on the `starter` plan
- a 1 GB disk mounted at `/var/data`
- a health check on `/healthz`
- the environment-variable slots listed below

The disk is what makes uploads survive a deploy. A service with a disk runs a
single instance and cannot do zero-downtime rollouts — that is the trade-off,
and it is why `numInstances` is pinned to 1.

## 3. Set the environment variables

Everything marked `sync: false` in `render.yaml` must be filled in from the
Render dashboard. Nothing secret belongs in the repository.

| Variable | Notes |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | From step 1. |
| `AdminSeed__Email` | First admin login. |
| `AdminSeed__Password` | Setting this is what enables admin seeding in production. |
| `Email__Smtp__Host` | e.g. `smtp.gmail.com`. Leave every SMTP value blank to disable outbound mail — the app then writes messages to disk instead of sending them. |
| `Email__Smtp__Port` | `587` |
| `Email__Smtp__User` | SMTP username. |
| `Email__Smtp__Password` | App password, never an account password. |
| `Email__FromAddress` | Envelope sender. |
| `Stripe__SecretKey` | Blank disables checkout. |
| `Stripe__PublishableKey` | |
| `Stripe__WebhookSecret` | Required by the `/NfcCard/StripeWebhook` endpoint. |

Double underscores map onto configuration sections: `Email__Smtp__Host`
resolves to `Email:Smtp:Host`.

## 4. After the first deploy

1. Watch the deploy log for `Database migrations applied on startup.`
2. Confirm `https://your-service.onrender.com/healthz` returns `Healthy` — it
   checks the database connection, not just the process.
3. Sign in with the admin credentials from step 3 and change the password.
4. Point the Stripe webhook at `https://your-service.onrender.com/NfcCard/StripeWebhook`.

## 5. Moving existing uploads

Photos and videos already committed under `wwwroot/startup-media` are baked
into the image and keep serving. Anything uploaded after deploy is written to
`/var/data/startup-media` on the disk. Both are served from the same
`/startup-media/…` URL, so nothing in the database needs rewriting.

Pitch decks live at `/var/data/pitch-decks` and are never web-served — they are
streamed through an authorized controller action.

---

## Configuration reference

| Setting | Default | Purpose |
| --- | --- | --- |
| `Storage__Root` | `/var/data` in the image | Base directory for uploads. Unset locally, so files stay in the project. |
| `Database__MigrateOnStartup` | `false` | Applies EF migrations during boot. `render.yaml` turns it on. |
| `PORT` | set by Render | The app binds `0.0.0.0:$PORT` when present. |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Development enables detailed error pages and admin seeding. |

## Running the container locally

```bash
docker build -t technova .
docker run --rm -p 8080:8080 \
  -e PORT=8080 \
  -e ConnectionStrings__DefaultConnection="…" \
  -v technova-data:/var/data \
  technova
```
