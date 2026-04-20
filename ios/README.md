# Monty (iOS)

Standalone **Autodesk OAuth** (PKCE + on-device token exchange) and **direct** hub list from `developer.api.autodesk.com`. **No local Node server** is required to sign in or pick a hub.

## Configure APS (once)

1. In your APS app, add callback **`monty://autodesk-oauth`** (same Client ID you use elsewhere).
2. In Xcode, set **`AUTODESK_CLIENT_ID`** in `Monty/Info.plist` (replace `YOUR_APS_CLIENT_ID`).

Optional: **`AUTODESK_SCOPE`** in Info.plist to override scopes (defaults match `aps-ai-web` minus `code:all`).

## Optional backend (chat / add-users via server)

Enter **Backend URL** in Settings only if you use **`aps-ai-web`** for:

- `POST /api/chat` (admin LLM)
- `POST /api/admin/add-users-to-projects`

Requests send **`Authorization: Bearer`** with the access token from Keychain. The server must allow Bearer auth (see `requireSession` in `aps-ai-web`).

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
