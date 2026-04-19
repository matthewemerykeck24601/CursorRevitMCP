# AdminChat (iOS)

Task-focused chat client for **APS / ACC admin workflows** backed by the same `POST /api/chat` endpoint as `aps-ai-web`, with `workspaceMode: "admin"` (for example: add users to projects via natural-language tasks).

## Open in Xcode

```bash
cd ios
open AdminChat.xcodeproj
```

Regenerate the project after editing `project.yml`:

```bash
cd ios && xcodegen generate
```

## Run against local `aps-ai-web`

1. Start the web app (from `aps-ai-web/`): `npm run dev`. For a **physical iPhone**, listen on all interfaces, e.g. `npx next dev -H 0.0.0.0 -p 3000`, and add the same callback URL with your Mac’s LAN IP in your APS app (for example `http://192.168.x.x:3000/auth/callback`).
2. **Simulator:** Settings → Base URL `http://127.0.0.1:3000`.
3. **Physical iPhone:** Base URL `http://<your-mac-lan-ip>:3000` (not `localhost`).
4. Tap **Sign in** → complete Autodesk OAuth in the embedded web view. Session cookies sync into the app for API calls.
5. Send admin tasks in the chat composer.

## Branch

Work for this app lives on `feature/ios-admin-chat`.
