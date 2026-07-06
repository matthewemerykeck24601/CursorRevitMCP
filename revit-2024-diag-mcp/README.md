# RevitDiag2024 — Read-Only Revit 2024 Diagnostic Bridge

A standalone, **read-only** MCP bridge that lets an MCP client (e.g. Claude Desktop)
observe a live **Revit 2024** session: watch journal logs, read model warnings, inspect
links, and query elements — **without writing anything to the model**.

It lives alongside the other Revit bridges under `D:\CursorRevitMCP`. It listens on
**port 14001** (the 2025 bridge uses 14000) and shares the same two-tier architecture:

```
MCP client  ──stdio──►  revit-2024-diag-mcp (TypeScript)  ──HTTP :14001──►  RevitDiag2024Bridge (C# add-in)  ──►  Revit 2024 API
```

- All spatial values at the MCP boundary are **millimetres**; the C# gateway converts to
  Revit internal feet via `mm / 304.8` (only relevant for grid endpoints / element centres
  here, since this bridge does not create geometry).
- The C# add-in opens **no `Transaction`** and exposes **no write/family-editor tools**.

## Components

| Path | What |
|------|------|
| `revit-2024-diag-mcp\` | TypeScript stdio MCP server (16 tools) |
| `RevitDiag2024Bridge\` | C# Revit 2024 add-in HTTP gateway (`net48`, port 14001) |

## Tools

**Core read (ported from the 2025 bridge):**
`say_hello`, `get_document_metadata`, `get_active_document_context`, `get_element_by_id`,
`get_element_metadata`, `get_element_types`, `get_levels`, `get_grids`, `query_elements`,
`get_current_view_elements`.

**Diagnostic (net-new):**
`get_warnings`, `get_missing_links`, `get_journal_tail`, `get_corrupt_elements`.

**Model element query & schedules (net-new):**
- `query_elements` — upgraded: filter by `category`, `familyName`, and `parameterFilters`
  (`{ name, value, operator }`, operator ∈ `equals` | `contains` | `startsWith` | `notEquals`,
  case-insensitive name matching against any instance **or** type parameter). Returns
  `elementId`, `category`, `familyName`, `typeName`, `level`, and a `parameters` object of
  every readable instance parameter (string-coerced). `limit` defaults to 100 when unspecified
  and has no maximum. This is the primary tool for the piece-mark split workflow (filter on
  `CONTROL_MARK`, `MANUFACTURE_COMPONENT`, `Assembly Name`, …).
- `get_schedules` — index of all non-template `ViewSchedule`s: `scheduleId`, `name`,
  `categoryName`, `rowCount` (data rows only).
- `get_schedule_data` — read one schedule's `TableData`, identified by `scheduleName` (first
  non-template match, case-insensitive — no `get_schedules` call needed) or `scheduleId`:
  `headers`, `rows` (header→cell maps), `rowCount`, `columnCount`, plus `truncated`/`totalRows`
  (cap 1000 rows, `maxRows` up to 5000).

## Build

C# add-in (Revit 2024 = .NET Framework 4.8):

```powershell
dotnet build "D:\CursorRevitMCP\RevitDiag2024Bridge\RevitDiag2024Bridge.csproj" -c Release
```

TypeScript server:

```powershell
cd D:\CursorRevitMCP\revit-2024-diag-mcp
npm install
npm run build
```

## Deploy (C# add-in)

```
%APPDATA%\Autodesk\Revit\Addins\2024\RevitDiag2024Bridge.addin           ← manifest
%APPDATA%\Autodesk\Revit\Addins\2024\RevitDiag2024Bridge\*.dll           ← assemblies
```

`RevitDiag2024Bridge.dll` ships with its `System.Text.Json` dependency chain — that package
is not part of the .NET Framework 4.8 BCL, so the DLLs must sit next to the add-in. Restart
Revit 2024; the gateway auto-starts on `http://127.0.0.1:14001` (log at
`%LOCALAPPDATA%\RevitDiag2024Bridge\bridge.log`).

## Register the MCP server (claude_desktop_config.json)

`%APPDATA%\Claude\claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "revit-2024-diag": {
      "command": "node",
      "args": ["D:\\CursorRevitMCP\\revit-2024-diag-mcp\\dist\\index.js"],
      "env": {
        "REVIT_2024_DIAG_URL": "http://127.0.0.1:14001"
      }
    }
  }
}
```

## Notes

- **Revit 2024 API**: targets `net48` and uses the int-based `ElementId` API
  (`IntegerValue` / `ElementId(int)`), which is deprecated-but-functional in 2024 (the
  long-based `Value` API is for Revit 2025+). The `CS0618` deprecation warnings are
  suppressed deliberately in the `.csproj`.
- `get_journal_tail` reads the newest `*.txt` in
  `%LOCALAPPDATA%\Autodesk\Revit\Autodesk Revit 2024\Journals\` with `FileShare.ReadWrite`
  so Revit's open write handle does not block it. It runs outside the Revit dispatcher (pure
  file IO).
- `get_corrupt_elements` is a best-effort scan (Name/Category access) capped at 1000
  elements; it is not a guarantee of model integrity.
