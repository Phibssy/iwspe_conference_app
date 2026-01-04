# Conference App

Minimal conference app scaffold:
- Frontend: Blazor WebAssembly (`client`) ✅
- Backend: Azure Functions (HTTP APIs) (`server`) ✅
- Storage: Azure Cosmos DB (free tier) ✅
- Auth: Azure AD for admin ✅
- CI/CD: GitHub Actions -> Azure Static Web Apps + Functions (free tiers) ✅

Note: registration model fields: `name` (required), `email` (required), `affiliation` (required), `phone` (optional), `selectedEvents` (array). The `dietary` field was removed.

## Quick start
1. Install .NET SDK (7+), Azure Functions Core Tools, and Azure CLI.
2. From `client` run: `dotnet new blazorwasm -o client` (or use the provided project files).
3. From `server` run: `func init --worker-runtime dotnet` then `func new --name RegistrationsFunction --template "HTTP trigger"`.
4. Configure `local.settings.json` with your Cosmos DB connection string and Azure AD settings (see `docs/setup.md`).

See `docs/setup.md` for full setup and deployment steps.
