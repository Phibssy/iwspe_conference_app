Title: Add tests & Static Web Apps deploy workflow (token-gated)

Summary:
- Adds unit tests for validation and capacity manager.
- Runs tests in CI and uploads test results.
- Adds a token-gated Static Web Apps deploy workflow (`deploy-staticwebapp.yml`) that will publish the client and server when `AZURE_STATIC_WEBAPP_API_TOKEN` is provided.

Checklist:
- [ ] Tests run in CI and pass
- [ ] Add `AZURE_STATIC_WEBAPP_API_TOKEN` as a secret when ready to deploy
- [ ] Confirm published artifacts are valid

Notes:
- Deploy step only runs when the secret is configured; it is safe to review before adding deploy secrets.