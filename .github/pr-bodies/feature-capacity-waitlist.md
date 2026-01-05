Title: Add capacity enforcement & waitlist handling (feature/capacity-waitlist)

Summary:
- Adds per-event `EventSelections` with statuses for `confirmed` / `waitlisted`.
- Adds `CapacityManager` to decide confirmed/waitlisted/rejected based on event capacity and waitlist capacity.
- Adds server-side enforcement toggled by `ENFORCE_CAPACITY` (env var).
- Adds `POST /api/registrations/promote/{eventId}` admin endpoint to promote the oldest waitlisted registration.
- Adds unit tests for capacity logic.

How to test locally:
- Run Cosmos DB Emulator and create `ConferenceDb` with `Program` and `Registrations` containers.
- Add a program document with events and `capacity` / `waitlistCapacity`.
- Set `ENFORCE_CAPACITY=true` in `server/local.settings.json`.
- Start functions: `cd server && func start` and register several attendees to exercise confirmed/waitlist/rejection behavior.
- Use admin UI or curl to call `POST /api/registrations/promote/{eventId}` to promote waitlists.

Checklist:
- [ ] Build passes (`dotnet build`)
- [ ] Tests pass in CI
- [ ] Admin promotion endpoint works and is protected
- [ ] Admin UI for managing waitlists (follow-up) added or planned

Notes:
- This change keeps old `selectedEvents` payloads supported for compatibility by mapping them to `EventSelections` on receive.
- I recommend reviewing the docs `docs/capacity.md` and `docs/admin.md` included in this branch.