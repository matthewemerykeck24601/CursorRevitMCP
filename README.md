# CursorRevitMCP

Workspace for APS / Autodesk Viewer integration, Alice (AI assistant), and Revit MCP workflows.

## Contents

- **`aps-ai-web/`** — Next.js app (OAuth, viewer, chat). See [`aps-ai-web/README.md`](aps-ai-web/README.md).
- **`.cursor/rules/`** — Cursor agent conventions for Revit MCP.
- **`docs/`** — Notes and playbooks.

## Setup

1. Copy `.cursor/mcp.json.example` to `.cursor/mcp.json` and adjust Revit / repo paths for your machine.
2. Copy `aps-ai-web/.env.example` to `aps-ai-web/.env.local` and set APS + AI keys.
3. Optional: repo-root `.env` is supported for shared secrets; it is gitignored.
4. From `aps-ai-web/`: `npm install` and `npm run dev`.

Do not commit `.env`, `.env.local`, or `.rvt` model files.
