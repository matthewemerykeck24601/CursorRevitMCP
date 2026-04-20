# Monty (iOS)

Standalone **Autodesk OAuth** (PKCE + on-device token exchange) and **direct** hub list from `developer.api.autodesk.com`. **No local Node server** is required to sign in or pick a hub.

## Configure APS (once)

1. In your APS app, add callback **`monty://autodesk-oauth`** (same Client ID you use elsewhere).
2. In Xcode, set **`AUTODESK_CLIENT_ID`** in `Monty/Info.plist` (replace `YOUR_APS_CLIENT_ID`).

Optional: **`AUTODESK_SCOPE`** in Info.plist to override scopes (defaults match `aps-ai-web` minus `code:all`).

## Optional backend

### Chat (Grok) — **`monty-ai-server`**

Recommended for **`POST /api/chat`**: run the standalone server in `../monty-ai-server` (xAI Grok, same API shape Monty expects). Example:

```bash
cd ../monty-ai-server && cp .env.example .env
# Set XAI_API_KEY in .env
npm install && npm run dev
```

In Monty **Settings**, set **Backend URL** to `http://127.0.0.1:8787` (or your machine’s LAN IP for a physical device).

### ACC admin — **`aps-ai-web`**

`POST /api/admin/add-users-to-projects` is still implemented in **`aps-ai-web`**. Point **Backend URL** at that deployment if you use add-users from the app.

All of these requests send **`Authorization: Bearer`** with the Autodesk access token from Keychain.

## Open in Xcode

```bash
cd ios
open Monty.xcodeproj
```

Regenerate after `project.yml` changes:

```bash
cd ios && xcodegen generate
```

## Branch

`feature/ios-admin-chat`
