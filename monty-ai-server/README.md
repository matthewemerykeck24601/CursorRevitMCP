# Monty AI server (Grok)

Minimal **HTTP API** for the Monty iOS app: **`POST /api/chat`** talks to **xAI** (Grok) using the same Responses endpoint pattern as `aps-ai-web`. No Next.js — run this process and point Monty’s **Backend URL** at it (for example `http://127.0.0.1:8787`).

## Setup

```bash
cd monty-ai-server
cp .env.example .env
# Edit .env — set XAI_API_KEY (and optionally XAI_MODEL)
npm install
npm run dev
```

## Environment

| Variable | Required | Description |
|----------|----------|-------------|
| `XAI_API_KEY` | **Yes** | xAI API key ([console.x.ai](https://console.x.ai/)) |
| `XAI_MODEL` | No | Defaults to `grok-3-latest` — use whatever your account supports |
| `PORT` | No | Default `8787` |
| `MONTY_REQUIRE_BEARER` | No | Default `1` — require `Authorization: Bearer` (Monty sends your APS access token) |

## Endpoints

- **`GET /health`** — liveness; reports whether `XAI_API_KEY` is set (not the value).
- **`GET /api/auth/session`** — same shape as `aps-ai-web` for optional session checks in Monty.
- **`POST /api/chat`** — JSON body compatible with Monty: `message`, `workspaceMode`, `chatHistory`, `selectedHubId`, optional `aiModel`.

Response: `{ "message": string, "requestId": string }`.

## iOS

In Monty **Settings**, set **Backend URL** to the server origin, e.g. `http://127.0.0.1:8787` (simulator) or your LAN IP for a device.

**Note:** “Add users to projects” and other ACC admin tools still live in **`aps-ai-web`** unless you add those routes here separately. This package is **chat-only** with a Grok-like tone.
