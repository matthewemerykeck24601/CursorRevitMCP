# Monty (iOS)

Native **Autodesk OAuth** (PKCE) and **direct** hub list from Autodesk. You can run everything **from your phone** without a Mac or Node server.

## 1. APS Client ID (OAuth) — Info.plist is correct

For a **public native app**, the APS **Client ID is not a secret** the way a `client_secret` is. Putting **`AUTODESK_CLIENT_ID`** in **`Monty/Info.plist`** (or in Xcode target → Info) is the normal approach — same idea as CastCam-style apps.

1. In the APS Developer Portal, register redirect **`monty://autodesk-oauth`** for that app.
2. Set **`AUTODESK_CLIENT_ID`** in `Monty/Info.plist` to your app’s Client ID.

Optional: **`AUTODESK_SCOPE`** to override scopes.

## 2. xAI (Grok) — Keychain, not plist

**Do not** put your **xAI API key** in Info.plist (it would ship in the IPA and is easy to extract).

Instead, in the app: **Settings → On-device Grok → paste API key → Save**. It is stored in the **Keychain** on device.

Optional: adjust **xAI model id** (default `grok-3-latest`).

Chat uses **on-device xAI** when a key is saved; it **does not** require Backend URL.

## 3. Add users to ACC projects — on-device

With **Backend URL empty**, **Add users to projects** calls Autodesk **Construction Admin** APIs directly using your **APS access token** (same endpoints as `aps-ai-web`).

- **Project numbers** use the same heuristic as the web app (first token of the Data Management project name).
- **Role IDs**: paste comma-separated **UUIDs** (ACC exposes these in admin). On-device, **role names** are not resolved (the web app uses server-side caches for name → id). If you only enter names, the app adds users **without** role assignment and explains in the JSON `note`.

If you set **Backend URL**, that flow still uses **`POST /api/admin/add-users-to-projects`** on your server (full name resolution like the web app).

## 4. Optional backend

- **monty-ai-server** or **aps-ai-web**: set **Backend URL** only if you want **server-side** chat or web parity. When an xAI key is saved, **on-device Grok takes priority** for chat.

## Open in Xcode

```bash
cd ios
xcodegen generate
open Monty.xcodeproj
```

## Branch

`feature/ios-admin-chat`
