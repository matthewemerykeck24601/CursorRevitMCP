# APS AI Viewer (Phase 1)

Minimal Next.js app that implements:

- Autodesk login with APS 3-legged OAuth
- Hub -> Project -> Model selection
- APS Viewer model loading
- AI chat + viewer action orchestration (read/query + viewer controls)
- Guarded write-action API contracts for Phase 2

## Requirements

- Node.js 20+
- APS app credentials
- APS callback URL configured for local dev

## Environment Setup

1. Create local env:

```bash
cp .env.example .env.local
```

2. Populate credentials:

- `APS_CLIENT_ID`
- `APS_CLIENT_SECRET`

3. Recommended callback URLs in APS app:

- `http://localhost:3000/auth/callback` (primary)
- `http://127.0.0.1:3000/auth/callback` (fallback)

Optional fallback dev host:

- `http://localhost:8888/auth/callback`
- `http://127.0.0.1:8888/auth/callback`

## Scope Baseline

Default scope set in `.env.example`:

- `data:read data:write data:create data:search`
- `bucket:create bucket:read bucket:update bucket:delete`
- `viewables:read account:read code:all`

## Run

```bash
npm install
npm run dev
```

Open `http://localhost:3000`.

## API Endpoints

### Auth

- `GET /auth/login`
- `GET /auth/callback`
- `GET /api/auth/session`
- `POST /api/auth/logout`

### APS

- `GET /api/aps/hubs`
- `GET /api/aps/hubs/:hubId/projects`
- `GET /api/aps/projects/:projectId/models?hubId=...`
- `GET /api/aps/viewer-token`

### Chat + Actions

- `POST /api/chat`
  - supports viewer commands: fit, clear selection, isolate, search/highlight
  - supports metadata query intent: list/model views
- `POST /api/actions/model`
  - write contract endpoint (`create|edit|delete`)
  - blocked by default in Phase 1
  - enable with `ENABLE_MODEL_WRITE_ACTIONS=true` for later implementations

## Request Tracing and Logs

- Request IDs are injected via middleware (`x-request-id`).
- API responses include `requestId` for troubleshooting.
- Structured server logs emit JSON lines.

## Troubleshooting

- `401 Not authenticated`
  - Re-run login (`/auth/login`), verify callback URL and session cookie.
- `Invalid OAuth state`
  - Ensure callback host matches login host and no mixed localhost/127 tabs.
- `Failed to load hubs/projects/models`
  - Verify APS scopes and your hub/project permissions.
- Viewer loads but model fails
  - Confirm selected item has a valid version and derivative availability.
- Chat actions do nothing in viewer
  - Ensure a model is loaded and ask commands like `find walls`, `fit view`.
