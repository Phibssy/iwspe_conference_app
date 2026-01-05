# Capacity & Waitlist — Usage Guide

This document explains how the capacity and waitlist features work, how to configure them, how to test locally using the Cosmos DB Emulator, and how to promote waitlisted attendees.

## Overview
- Each event in program documents (`Program` container) contains `Capacity` and `WaitlistCapacity` fields.
  - `Capacity` (integer): 0 means unlimited.
  - `WaitlistCapacity` (integer): 0 means no waitlist.
- The registration flow assigns each requested event a status: `confirmed`, `waitlisted`, or the registration is rejected for that event.
- Capacity enforcement is toggleable with an environment variable `ENFORCE_CAPACITY` (default: false).

## Configuration
- Local (emulator): set `ENFORCE_CAPACITY` in `server/local.settings.json`:

```
"ENFORCE_CAPACITY": "true"
```

- Production (Function App): set `ENFORCE_CAPACITY=true` as an Application Setting.

## Program event example

```json
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

## Registering attendees
- POST /api/registrations accepts either `selectedEvents` (legacy string array) or `eventSelections` (preferred) in the body. The server maps `selectedEvents` to `eventSelections` during processing.

Example request:

```json
{
  "name": "Alice Example",
  "email": "alice@example.edu",
  "affiliation": "Example University",
  "selectedEvents": ["event-123"]
}
```

Successful response when confirmed/waitlisted:

```json
HTTP/1.1 201 Created
{
  "message": "registration received",
  "id": "<registration-id>",
  "selections": [ { "eventId": "event-123", "status": "confirmed", "registeredAt": "2026-01-05T12:00:00Z" } ]
}
```

If an event and its waitlist are full, the server will respond with 409 Conflict and list rejected events:

```json
HTTP/1.1 409 Conflict
{
  "error": "events full",
  "events": ["event-123"]
}
```

## Promote waitlist (admin)
- Endpoint: POST `/api/registrations/promote/{eventId}`
- This is a protected admin API (requires a valid Azure AD access token for the app). It promotes the oldest waitlisted registration for that event to `confirmed`.

Example using curl (replace placeholders):

```bash
curl -X POST "https://<functions-host>/api/registrations/promote/event-123" \
  -H "Authorization: Bearer <ADMIN_ACCESS_TOKEN>"
```

Response on success:

```json
HTTP/1.1 200 OK
{ "message": "promoted", "id": "<registration-id>", "eventId": "event-123" }
```

## Testing locally with Cosmos DB Emulator
1. Install and run the emulator on Windows (see https://learn.microsoft.com/azure/cosmos-db/local-emulator).
2. Create `ConferenceDb` and containers `Program` and `Registrations` (partition key `/id`).
3. Insert sample program documents including event objects with `id`, `capacity`, and `waitlistCapacity`.
4. Set `ENFORCE_CAPACITY=true` in `server/local.settings.json` and run the Functions host (`cd server && func start`).
5. Use the Blazor admin UI or `curl` to register and test waitlisting and promotion.

## Notes
- The admin UI will be extended to show per-event confirmed and waitlisted counts and provide a one-click promote action for maintainers.
- All changes are non-destructive: previous `selectedEvents` payloads remain supported for compatibility.