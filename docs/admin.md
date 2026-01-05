# Admin Guide

This guide describes admin tasks: uploading program documents, inspecting registrations, and managing waitlists.

## Uploading & updating the program
- Admin can upload program JSON via `POST /api/program/upload` (protected).
- The body should be a `ProgramDocument` with an `Events` array; each `EventItem` should include `id`, `name`, `type`, `departure`, `return`, `capacity`, and `waitlistCapacity`.

## Viewing registrations
- Admin can view all registrations via `GET /api/registrations` (protected).
- Registration objects include `EventSelections` with `EventId`, `Status`, and `RegisteredAt`.

## Promoting waitlisted attendees
- Use the `POST /api/registrations/promote/{eventId}` admin endpoint to promote the oldest waitlisted registration.
- The endpoint requires an Authorization Bearer token (Azure AD access token with `access_as_user` scope configured for the app).

Example token acquisition (developer note):
- Use the Blazor admin UI sign-in which will request the API scope `api://<client-id>/access_as_user` at login, then call the promote endpoint with the acquired access token.

## Admin UI (next steps)
- I will implement a small admin UI panel that will:
  - Show event list with capacity and waitlist counts
  - Show list of waitlisted registrations per event (with "promote" button)
  - Allow CSV export of registrations
  - Allow toggling `ENFORCE_CAPACITY` in environment (admin controlled)

This UI will be added as a follow-up to the `feature/capacity-waitlist` branch and we will add UI tests for the workflows.