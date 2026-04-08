# Phase 1 Runbook

## Local Startup

1. Ensure `.env.local` exists with APS credentials.
2. Start app:
   - `npm run dev`
3. Open `http://localhost:3000`.
4. Click `Login with Autodesk`.
5. Select Hub -> Project -> Model.
6. Confirm viewer status transitions to `Model loaded`.

## Quick Chat Commands

- `fit view`
- `clear selection`
- `isolate`
- `find walls`
- `list model views`

## Response Diagnostics

- Every API response includes `requestId`.
- Server logs emit JSON with `requestId` for correlation.
- For auth/session issues, check `GET /api/auth/session`.

## Phase 1 Guardrails

- Write actions are contract-only and blocked by default:
  - `POST /api/actions/model`
  - returns `403` unless `ENABLE_MODEL_WRITE_ACTIONS=true`

## Next Integration Targets

- AEC Data Model GraphQL query route
- Data Exchange query route
- Cursor Cloud Agent provider abstraction for chat
