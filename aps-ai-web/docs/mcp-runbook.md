# APS AI Web MCP Runbook

## Scope
- MCP runtime: Node.js + TypeScript
- Fixture strategy: currently loaded model
- Read behavior: targeted-minimal by default
- Write behavior (v1): parameter edits only, guarded two-step confirmation

## Start MCP Server (local)
1. Install dependencies in `aps-ai-web/mcp-server`:
   - `npm install`
2. Run dev server:
   - `npm run dev`

## Cursor MCP Registration
Add an MCP entry in `.cursor/mcp.json`:
- name: `aps-ai-web-mcp`
- command: `cmd /c npx -y tsx d:\\CursorRevitMCP\\aps-ai-web\\mcp-server\\src\\index.ts`

## Available Tools
- `get_selected_elements`
- `get_element_parameters`
- `search_elements`
- `list_model_views`
- `viewer_action`
- `set_element_parameters_guarded`

## Guarded Write Flow
1. Call `set_element_parameters_guarded` with `apply=false` and desired `changes`.
2. Receive `confirmationToken`.
3. Call again with `apply=true` and exact `confirmationToken`.
4. If token mismatch, operation is blocked.

## Verification Checklist
- Read tools return expected selected element data.
- Parameter query resolves control and dimension fields.
- Search returns expected dbIds and count.
- Guarded write blocks without valid token and allows with valid token.
