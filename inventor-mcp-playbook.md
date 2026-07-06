# Inventor MCP Playbook (Phase 1)

## Goal

Establish a repeatable flow where:

1. Revit family parameter data is extracted from running Revit (existing MCP stack).
2. Inventor receives mapped parameters and iLogic updates through `inventor-mcp`.
3. Parameter sets are synchronized to Excel as the source of truth.
4. Informed Design publish is completed manually from Inventor.

## Components

- `inventor-mcp` (Node MCP stdio server)
- `InventorMCPBridge` (Inventor add-in local HTTP gateway)
- Existing Revit MCP endpoints for parameter extraction
- Excel workbook for parameter maintenance (`INVENTOR_EXCEL_PATH`)

## Startup

1. Build and deploy `InventorMCPBridge` add-in to Inventor add-ins location.
2. Launch Inventor and confirm bridge gateway is running (`http://127.0.0.1:8776/health` by default).
3. In repo root, run `npm install` in `inventor-mcp` once.
4. Add `inventor-mcp` entry from `.cursor/mcp.json.example` into `.cursor/mcp.json`.
5. Reload Cursor MCP servers.

## Core Tooling Flow

1. `inventor_health`
2. `inventor_get_active_document`
3. `revit_extract_family_parameters` (normalize source payload)
4. `inventor_replicate_from_revit_payload`
5. `inventor_push_parameters_to_excel` and `inventor_sync_parameters`
6. `inventor_prepare_informed_design_publish`
7. Publish manually via Informed Design add-in in Inventor

## Recommended Run Label

For traceability add a run label in comments or metadata:

- `MCP_RUN::<yyyyMMdd-HHmm>::<productCode>`

Use the same run label in:

- Inventor document iProperties/comments
- Excel row metadata
- publish checklist notes

## Troubleshooting

- `inventor_health` fails:
  - verify `INVENTOR_BRIDGE_URL` and gateway port in bridge settings
  - check `%AppData%/InventorMCPBridge/bridge.log`
- iLogic calls fail:
  - ensure iLogic add-in is enabled in Inventor
  - confirm active document is writable and not read-only
- parameter sync drift:
  - pull from Excel first, then set in Inventor, then push back after approved edits
