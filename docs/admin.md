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

## Admin UI (available now)
- A small Admin Waitlist Manager has been added at the route `/admin/waitlist`.
  - It lists events (title, capacity), shows counts for confirmed & waitlisted registrations, and provides a **Promote** button to promote the next waitlisted attendee for that event.
  - The Admin page is protected by Azure AD; you must sign-in as an admin and have the API scope configured to call `POST /api/registrations/promote/{eventId}`.

Tips:
- Open the app and navigate to `/admin`, then click **Open Waitlist Manager**.
- The Promote operation calls the API and refreshes counts on success.