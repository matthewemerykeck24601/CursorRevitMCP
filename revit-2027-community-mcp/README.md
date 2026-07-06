# revit-2027-community-mcp

Community write-tool companion MCP server for Revit 2027.

This server does not replace Autodesk's `revit-public-mcp`. It complements it by routing write-oriented tool calls through the local `RevitPublicMCPBridge` add-in gateway.

## Implemented tools

- `say_hello`
- `get_current_view_elements`
- `get_available_family_types`
- `create_point_based_element`

## Runtime requirements

- Revit 2027 running with `RevitPublicMCPBridge` loaded
- Bridge gateway enabled in add-in settings (default on)
- Gateway URL reachable (default `http://127.0.0.1:8766`)

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

## Cursor MCP config snippet

```json
{
  "mcpServers": {
    "revit-2027-community-mcp": {
      "command": "cmd",
      "args": ["/c", "npx", "-y", "tsx", "D:\\CursorRevitMCP\\revit-2027-community-mcp\\src\\index.ts"],
      "env": {
        "REVIT_2027_BRIDGE_URL": "http://127.0.0.1:8766"
      }
    }
  }
}
```
