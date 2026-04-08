# Phase 1 Task List (APS Viewer + AI Chat)

## Objective

Deliver a minimal webapp that supports:

- Login
- Hub selection
- Model selection
- Viewer shell
- AI chat panel connected to model context

## Stack

- Next.js full-stack (TypeScript)
- APS OAuth + Data Management + Model Derivative
- Viewer command bus
- Chat orchestration endpoint (Cursor Cloud Agent ready)

## Execution Tasks

- [ ] Scaffold app structure in `aps-ai-web`
- [ ] Add environment contract (`APS_CLIENT_ID`, `APS_CLIENT_SECRET`, callback/scopes)
- [ ] Implement OAuth routes:
  - [ ] `GET /auth/login`
  - [ ] `GET /auth/callback`
  - [ ] session check + logout route
- [ ] Build APS proxy routes:
  - [ ] hubs list
  - [ ] projects list by hub
  - [ ] model list by project
  - [ ] viewer token endpoint
- [ ] Build page layout:
  - [ ] auth state header
  - [ ] hub/project/model selectors
  - [ ] viewer container
  - [ ] chat panel
- [ ] Add viewer shell:
  - [ ] Autodesk Viewer loader
  - [ ] model load by selected item/version URN
  - [ ] basic commands (`select`, `isolate`, `fit`)
- [ ] Add chat API and tool dispatch:
  - [ ] parse prompt intent
  - [ ] call read/query tools
  - [ ] dispatch viewer commands
- [ ] Add telemetry + error handling:
  - [ ] request IDs
  - [ ] typed API error payloads
  - [ ] user-facing status messages
- [ ] Validate local run:
  - [ ] `http://localhost:3000`
  - [ ] callback success at `/auth/callback`
  - [ ] end-to-end flow test

## Callback URLs (Primary + Fallback)

- Primary:
  - `http://localhost:3000`
  - `http://localhost:3000/auth/callback`
- Fallback:
  - `http://localhost:8888`
  - `http://localhost:8888/auth/callback`
  - `http://127.0.0.1:8888`
  - `http://127.0.0.1:8888/auth/callback`

## Scope Baseline

Use full baseline requested for Phase 1:

- `data:read data:write data:create data:search`
- `bucket:create bucket:read bucket:update bucket:delete`
- `viewables:read account:read code:all`

## Exit Criteria

- User can authenticate
- User can pick hub and model
- Viewer loads selected model
- Chat can trigger at least 3 deterministic viewer/model actions
