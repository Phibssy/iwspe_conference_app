Title: Fix CI YAML (syntax) and add deploy placeholder

Summary:
- Fixes a YAML syntax issue in `.github/workflows/ci-cd.yml` and adds a safe, token-gated deploy placeholder to avoid accidental deploys.

Checklist:
- [ ] CI workflow file validated and passes
- [ ] No remaining YAML syntax errors

Notes:
- The deploy job is disabled by default (`if: false`) and the Static Web Apps deploy workflow only runs when `AZURE_STATIC_WEBAPP_API_TOKEN` is configured as a GitHub secret.