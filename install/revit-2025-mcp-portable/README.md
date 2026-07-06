# Revit 2025 MCP — Portable Installer

Connect **Revit 2025** to **Cursor** or **Claude Desktop** via the local MCP bridge. This package installs:

1. **RevitPublicMCPBridge** add-in (HTTP gateway inside Revit)
2. **revit-2025-bridge-mcp** Node MCP server (stdio tools for AI clients)

---

## Can I install from any drive or folder?

**Yes.** There are no hardcoded `C:\` or repo paths in the installed result.

| What | Where it ends up |
|------|------------------|
| **Installer / zip** | Run from **any drive, any folder** (e.g. `E:\Tools\Revit2025-MCP-Portable`, `D:\Downloads\...`) |
| **Revit add-in** | Always `%AppData%\Autodesk\Revit\Addins\2025\` (your Windows user profile) |
| **MCP server (runtime)** | Always `%LOCALAPPDATA%\Revit2025Mcp\bridge-mcp\` |
| **Claude / Cursor config** | Points to `%LOCALAPPDATA%\Revit2025Mcp\...` — **not** the folder you extracted the zip into |

After install, you may **delete or move** the original installer folder; Revit and MCP clients use the `%AppData%` / `%LOCALAPPDATA%` copies.

**Portable zip:** extract anywhere → run `Install-Revit2025Mcp.ps1 -SkipBuild` (portable mode is auto-detected when `artifacts\` and `revit-2025-community-bridge-mcp\` sit next to the script).

**Full repo:** clone the repo to any drive → run from `install\revit-2025-mcp-portable\` (builds the add-in with `dotnet`).

**`-ConfigureCursor` note:** if you use this flag, pass your real workspace with  
`-CursorProjectPath "D:\YourProject"` — otherwise it writes `.cursor\mcp.json` into the installer folder.

---

## Requirements

| Requirement | Notes |
|-------------|--------|
| **Autodesk Revit 2025** | 64-bit Windows |
| **Node.js 20+** | [https://nodejs.org](https://nodejs.org) — needed for `npx` / MCP server |
| **.NET SDK 8+** | Only if building from source (not needed for pre-built portable zip) |
| **Cursor or Claude Desktop** | For AI chat with MCP tools |

**Port:** Revit 2025 gateway uses **8767** (Revit 2026 uses 8766 — both can run together).

---

## Quick install (recommended)

1. **Close Revit 2025** completely (tray included).
2. Open **PowerShell** (not necessarily admin).
3. Run:

```powershell
cd path\to\install\revit-2025-mcp-portable

# From full repo (builds add-in):
.\Install-Revit2025Mcp.ps1 -ConfigureClaude -ConfigureCursor

# From pre-built portable zip (no dotnet build):
.\Install-Revit2025Mcp.ps1 -SkipBuild -ConfigureClaude -ConfigureCursor
```

4. **Start Revit 2025** — confirm the **MCP** ribbon tab appears.
5. **Restart** Claude Desktop and/or reload MCP in Cursor.

---

## What the installer does

| Step | Location |
|------|----------|
| Revit add-in DLL (versioned) | `%AppData%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge-v2\` |
| Add-in manifest | `%AppData%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge.addin` |
| Bridge settings (port 8767) | `%AppData%\RevitPublicMCPBridge\settings-2025.json` |
| MCP server copy | `%LOCALAPPDATA%\Revit2025Mcp\bridge-mcp\` |
| Optional Claude config | `%AppData%\Claude\claude_desktop_config.json` |
| Optional Cursor config | `<your-project>\.cursor\mcp.json` |

Each install writes a **new versioned DLL** (`RevitPublicMCPBridge.net8.v5.dll`, etc.) so deploy works even if an older DLL was locked by Revit.

---

## MCP client configuration (manual)

If you skip `-ConfigureClaude` / `-ConfigureCursor`, add this yourself.

**Server name:** `revit-2025-bridge-mcp`

**Cursor** — `.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "revit-2025-bridge-mcp": {
      "command": "cmd",
      "args": [
        "/c",
        "npx",
        "-y",
        "tsx",
        "C:\\Users\\YOU\\AppData\\Local\\Revit2025Mcp\\bridge-mcp\\src\\index.ts"
      ],
      "env": {
        "REVIT_2025_BRIDGE_URL": "http://127.0.0.1:8767"
      }
    }
  }
}
```

Replace the path with your `%LOCALAPPDATA%\Revit2025Mcp\bridge-mcp` folder.

**Claude Desktop** — `%AppData%\Claude\claude_desktop_config.json` — same `mcpServers` block under the existing JSON (keep `preferences` etc.).

After editing: **fully quit and reopen** Claude Desktop, or reload MCP in Cursor.

---

## Verify installation

### 1. Gateway health (Revit running)

PowerShell:

```powershell
Invoke-WebRequest http://127.0.0.1:8767/health -UseBasicParsing
```

Expect `"revitConnected": true` when a document is open.

### 2. MCP tools

In Cursor or Claude, confirm server **`revit-2025-bridge-mcp`** lists tools including:

| Tool | Use when |
|------|----------|
| `say_hello` | Test connection |
| `get_document_metadata` | Project / file info |
| `get_element_metadata` | Selection or element IDs in a **project** |
| **`get_family_parameters`** | **Family Editor** — family type/instance params & formulas |
| `get_current_view_elements` | Elements in active view |

### 3. Family Editor test

1. Open a `.rfa` in Revit 2025 Family Editor.
2. Ask your AI client to call **`get_family_parameters`** with:
   ```json
   { "parameterNames": ["DIM_LENGTH"] }
   ```
   Or omit `parameterNames` to return all family parameters.

---

## Building a distributable zip (IT / developers)

On a machine with Revit 2025 + .NET SDK:

```powershell
cd path\to\CursorRevitMCP\install\revit-2025-mcp-portable
.\Build-PortablePackage.ps1
```

Output folder:

`install\revit-2025-mcp-portable\dist\Revit2025-MCP-Portable\`

Zip and send to end users. They run:

```powershell
.\Install-Revit2025Mcp.ps1 -SkipBuild -ConfigureClaude
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| Add-in startup error / port conflict | Another Revit year may own 8766; 2025 uses **8767**. Close other Revit versions or check Settings. |
| Deploy failed / DLL locked | Close Revit 2025, re-run installer (creates next `vN.dll`). |
| MCP tool missing after update | Full restart of Claude Desktop or reload MCP in Cursor. Tool name is exactly **`get_family_parameters`**. |
| Tool listed but `fetch failed` | Revit not running or gateway off — start Revit 2025 with a document open. |
| `get_family_parameters` 404 | Old add-in still loaded — **restart Revit 2025** after install. |
| Claude web (claude.ai) | Browser cannot reach localhost; use **Claude Desktop**. |

**Logs:** `%AppData%\RevitPublicMCPBridge\bridge.log`

---

## Uninstall

1. Delete `%AppData%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge.addin`
2. Delete `%AppData%\Autodesk\Revit\Addins\2025\RevitPublicMCPBridge-v2\` (optional)
3. Delete `%LOCALAPPDATA%\Revit2025Mcp\` (optional)
4. Remove `revit-2025-bridge-mcp` from Claude/Cursor MCP config

---

## Support

Package version matches repository **revit-2025-community-bridge-mcp** (18 tools).  
For formula diff workflows, use **`get_family_parameters`** on each family document separately, then compare JSON in your AI client.
