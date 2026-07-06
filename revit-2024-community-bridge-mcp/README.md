# Revit 2025 Community Bridge MCP

Stdio MCP server that talks to **RevitPublicMCPBridge** running inside Revit 2025 on the local HTTP gateway (`http://127.0.0.1:8767` by default — avoids conflict with Revit 2026 on 8766).

## Metadata / provenance tools

| Tool | Purpose |
|------|---------|
| `get_document_metadata` | Project path, worksharing central, file last-saved user/time, project information fields, current Revit user |
| `get_element_metadata` | Per-element worksharing tooltip (creator, last changed by, owner), checkout status, central sync state (`ModelUpdatesStatus`), `VersionGuid`, optional parameter map |
| `get_family_parameters` | Family Editor only: `FamilyManager` parameters (name, type, formula, per-type values). Optional `parameterNames` filter |

**Limits:** Revit does not expose a full per-element edit audit log or “who reloaded” in the public API. Reload/sync history is model-level (Synchronize with Central), not per element. Use worksharing fields above plus optional `Comments` parameters for traceability.

## Portable installer (end users)

For a self-contained Windows install package (add-in + MCP server + README):

```powershell
cd install\revit-2025-mcp-portable
.\Install-Revit2025Mcp.ps1 -ConfigureClaude -ConfigureCursor
```

Pre-built zip: `install/revit-2025-mcp-portable/dist/Revit2025-MCP-Portable.zip`  
See **`install/revit-2025-mcp-portable/README.md`** for full instructions.


1. Revit **2025** with **RevitPublicMCPBridge** add-in installed and gateway enabled (Settings → local API gateway on port **8767** when Revit 2026 is also running).
2. Node.js 20+ (for `npx tsx`).

## Build the add-in for Revit 2025

```powershell
dotnet build D:\CursorRevitMCP\RevitPublicMCPBridge\RevitPublicMCPBridge.csproj -c Release -p:RevitYear=2025
```

Deploy `bin\Release\net8.0-windows\RevitPublicMCPBridge.dll` and copy `RevitPublicMCPBridge.2025.addin` to:

`%AppData%\Autodesk\Revit\Addins\2025\`

Update the `<Assembly>` path in the `.addin` file to your deploy folder.

## Cursor MCP config

Add to `.cursor/mcp.json` (see repo `.cursor/mcp.json.example`):

```json
"revit-2025-bridge-mcp": {
  "command": "cmd",
  "args": ["/c", "npx", "-y", "tsx", "D:\\CursorRevitMCP\\revit-2025-community-bridge-mcp\\src\\index.ts"],
  "env": {
    "REVIT_2025_BRIDGE_URL": "http://127.0.0.1:8767"
  }
}
```

## Example: selected element provenance

1. Select element(s) in Revit 2025.
2. Call MCP tool `get_element_metadata` with `{ "useSelection": true, "includeParameters": true }`.

Returns worksharing owner/creator/last-changed-by when the model is workshared, plus checkout and central-update status.
