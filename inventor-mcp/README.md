# inventor-mcp

MCP stdio server for Autodesk Inventor workflows:

- write and run iLogic in the active Inventor document
- replicate Revit family parameter payloads into Inventor parameters
- sync Inventor parameter sets with an Excel-based data source
- validate publish readiness for manual Informed Design publishing

## Requirements

- Node.js 20+
- Running `InventorMCPBridge` add-in with local gateway enabled
- `INVENTOR_BRIDGE_URL` reachable (default `http://127.0.0.1:8776`)

## Development

```bash
npm install
npm run dev
```

## Environment

Copy `.env.example` and configure:

- `INVENTOR_BRIDGE_URL` - local gateway URL
- `INVENTOR_PARAM_SOURCE` - `excel` (phase 1 default) or `sql` (stub)
- `INVENTOR_EXCEL_PATH` - path to parameter workbook

## Key Tools

- `inventor_health`
- `inventor_get_active_document`
- `inventor_run_ilogic_rule`
- `inventor_show_ilogic_form`
- `inventor_write_ilogic_rule`
- `inventor_get_parameters`
- `inventor_set_parameters`
- `revit_extract_family_parameters`
- `inventor_replicate_from_revit_payload`
- `inventor_pull_parameters_from_excel`
- `inventor_push_parameters_to_excel`
- `inventor_sync_parameters`
- `inventor_prepare_informed_design_publish`
