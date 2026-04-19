# Monty (iOS)

Task-focused chat for **APS / ACC admin** workflows via `POST /api/chat` with `workspaceMode: "admin"` and **`selectedHubId`** from your chosen hub.

## Open in Xcode

```bash
cd ios
open Monty.xcodeproj
```

Regenerate the project after editing `project.yml`:

```bash
cd ios && xcodegen generate
```

## First launch flow

1. **Server URL** — Point Monty at your running `aps-ai-web` (e.g. `http://127.0.0.1:3000` in Simulator, or your Mac’s LAN IP on a device with `npx next dev -H 0.0.0.0 -p 3000`).
2. **Autodesk sign-in** — Embedded browser OAuth (same cookies as the web app). Monty calls `GET /api/auth/session` to verify the session.
3. **Hub** — `GET /api/aps/hubs` lists hubs; pick one and tap **Use this hub**. Hub id + name are stored in **UserDefaults** (`monty.selectedHubId` / `monty.selectedHubName`).
4. **Chat** — Messages include `selectedHubId` for admin context.

5. **Add users to projects** — Toolbar **person.badge.plus** opens a form that calls `POST /api/admin/add-users-to-projects` on `aps-ai-web`. That uses the same server code as the web/chat tool `admin_add_users_to_projects` (`addUsersToProjectsByNumber`); **no MCP runs on the device**.

Later launches: if the session is still valid, you go straight to chat with the same hub. If the session expires (`401` from chat or hubs), Monty returns to sign-in; **hub selection is kept** so you only sign in again.

**Settings** (gear): change base URL, **Change hub…**, or **Sign out** (clears cookies; hub choice remains).

## Branch

Work for this app lives on `feature/ios-admin-chat`.
