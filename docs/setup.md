# Setup & Deployment Guide

## Prereqs
- .NET 7 SDK
- Azure CLI (az)
- Azure Functions Core Tools (optional for local testing)
- GitHub account (for CI & Azure Static Web Apps deployment)

## Azure Resources (free tiers)
1. Create a Resource Group.
2. Create a Cosmos DB (Core (SQL)) account and set capacity to free tier if available.
3. Create an Azure Static Web App (link to repo) and connect to GitHub Actions.
4. Create Azure Function App for the backend.
5. Configure Azure AD (Microsoft Entra ID):
   - Register an App (Web) for Admin sign-in.
   - Note **Tenant ID**, **Client ID**, and create a **client secret** (store securely).
   - Configure Redirect URIs for your admin UI (e.g. `https://localhost:5001/authentication/login-callback` for local Blazor testing and your Static Web App production URL for deployment).
   - Required scopes: `openid`, `profile`, `email` (optionally `offline_access` to enable refresh tokens for long sessions).
   - Environment variables / GitHub Secrets to provide:
     - `AZURE_AD_TENANT_ID` — tenant id
     - `AZURE_AD_CLIENT_ID` — app (client) id
     - `AZURE_AD_CLIENT_SECRET` — client secret (server-side usage)
   - Notes on usage:
     - Blazor WebAssembly uses MSAL (client-side): configure `AZURE_AD_TENANT_ID` and `AZURE_AD_CLIENT_ID` in `appsettings.json` or as pipeline-managed secrets during build.
     - Azure Functions (server) can validate access tokens or use Easy Auth / App Service Authentication. For server-to-server operations that require a secret, set `AZURE_AD_CLIENT_SECRET` as a function app setting (or GitHub Action secret for deployment).
     - Grant the app the delegated permissions `openid profile email` and add an API scope if you want to secure function APIs via tokens (e.g., `api://<client-id>/access_as_user`).
     - I will wire authentication to expect these env vars and use them in MSAL configuration and token validation when you provide the secrets.

   - Supplying credentials via GitHub Actions / Azure App Settings:
     - In GitHub: go to Settings → Secrets → Actions and add `AZURE_AD_TENANT_ID`, `AZURE_AD_CLIENT_ID`, `AZURE_AD_CLIENT_SECRET` (and any other Azure secrets like `AZURE_CREDENTIALS`, `AZURE_STATIC_WEBAPP_API_TOKEN`).
     - In Azure: set the same keys as Application Settings for your Function App (these become environment variables available to the app at runtime).
     - For local development: set values in `server/local.settings.json` (do not commit this file).
     - When you create the client secret in the Entra app, paste it into the GitHub Secret `AZURE_AD_CLIENT_SECRET` — I will read the secret from env vars during deployment and configure MSAL/token validation accordingly.

## Notes on the MSAL / API integration
- The Blazor admin app is configured to use MSAL (client-side) and requests the API scope: `api://<client-id>/access_as_user` (this is shown in `client/wwwroot/appsettings.json`).
- You should define the API scope on the app registration (Expose an API → Add a scope → `access_as_user`) so the SPA can request an access token for the backend.
- After the API scope exists, grant delegated permission by adding it to the SPA registration or by granting admin consent.
- The Functions backend validates incoming access tokens using the tenant and client id environment variables and rejects unauthenticated requests to admin endpoints (`/api/program/upload`, `/api/registrations` GET). Public endpoints (e.g., `/api/busschedule` and `POST /api/registrations`) remain public where appropriate.
- When you’re ready, create the API scope and paste the client secret to GitHub Secrets; I’ll then complete a full token flow test and open the PR.

## Deployment
- Push to GitHub and open a PR; CI will run builds, then deploy when you configure deployment secrets.

## Next steps
- Implement Cosmos DB integration with `Azure.Cosmos` using environment variables. (Done: service and function wiring added.)
- Add Azure AD authentication in the client (MSAL) and secure admin routes.
- Add tests and capacity handling / waitlists for social events.

## Registration fields
The registration form collects the following fields (finalized):

- `name` (required)
- `email` (required)
- `affiliation` (e.g., university) (required)
- `phone` (optional)
- `selectedEvents` (array of event IDs the attendee signs up for)

Example registration JSON:

```
{
  "name": "Alice Example",
  "email": "alice@example.edu",
  "affiliation": "Example University",
  "phone": "+44 7123 456789",
  "selectedEvents": ["event-123", "event-456"]
}
```

## Capacity & waitlist configuration (how to set up)
- Event capacity and waitlist settings are stored per event in the program documents (in the `Program` container). Each `EventItem` has `Capacity` and `WaitlistCapacity` fields. Example event JSON:

```
{
  "id": "event-123",
  "name": "Bus to Kühtai",
  "type": "bus",
  "departure": "2026-02-15T08:00:00",
  "return": "2026-02-15T17:00:00",
  "capacity": 35,
  "waitlistCapacity": 10
}
```

- Enforcement strategy (apply later):
  - Option A (server-enforced): set `ENFORCE_CAPACITY=true` as an app setting. When enabled the registration endpoint will check the number of existing confirmed registrations for the event and reject new registrations past capacity (or place them on the waitlist if waitlistCapacity > 0).
  - Option B (admin-enforced / manual): keep `ENFORCE_CAPACITY=false` and use the admin UI to stop accepting registrations for full events manually.

- Implementation notes:
  - When you want enforcement enabled, I will add logic in `Register` that queries existing registrations for selected event(s), compares with `Capacity` and `WaitlistCapacity`, and returns `409 Conflict` for fully booked events (or `202 Accepted` with `waitlisted` flag for waitlist entries).
  - The code will record whether a registration is `confirmed` or `waitlisted` so the admin UI can manage and promote waitlisted attendees when slots free up.


