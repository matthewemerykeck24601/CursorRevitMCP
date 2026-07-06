# revit-2026-community-bridge-mcp

Community write-tool companion MCP server for Revit 2026.

This server complements Autodesk's `revit-public-mcp` and `user-revit-2026-community-mcp` by exposing bridge-backed project write tools, including shared parameter creation and binding.

## Implemented tools

- `say_hello`
- `get_current_view_elements`
- `get_available_family_types`
- `create_point_based_element`
- `open_selected_family_editor`
- `open_family_editor_by_element_id`
- `ensure_shared_parameters`
- `bind_shared_parameters`
- `add_shared_params_to_family_library`
- `extract_family_library_parameters`
- `extract_family_desc_variants`

## Runtime requirements

- Revit running with a gateway bridge exposing the routes used by this server
- Bridge URL reachable (default `http://127.0.0.1:8766`)

## Local development

```powershell
npm install
npm run dev
```

## Build

```powershell
npm install
npm run build
npm run start
```
