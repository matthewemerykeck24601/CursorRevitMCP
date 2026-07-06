# RevitPublicMCPBridge (Revit 2027)

Professional ribbon bridge add-in for Autodesk Revit 2027 that controls the official Public MCP Server Tech Preview.

## Features

- Dedicated `MCP` tab and `Public MCP Server` panel
- Large buttons:
  - `Start Server`
  - `Stop Server`
  - `Status`
  - `Copy Config`
  - `Settings`
  - `Help / Docs`
- Auto-start option on Revit launch
- Graceful server stop on Revit shutdown (configurable)
- Fallback control strategy:
  - Reflection binding to in-process Autodesk MCP APIs if available
  - Process-based start/stop/status (`Autodesk.RevitMcpServer.Stdio.exe`) otherwise
- Robust logging to `%AppData%\RevitPublicMCPBridge\bridge.log`
- Settings persisted at `%AppData%\RevitPublicMCPBridge\settings.json`

## Project Layout

- `RevitPublicMCPBridge.csproj`
- `RevitPublicMCPBridge.cs` (IExternalApplication + ribbon)
- `Commands/` (button handlers)
- `Services/` (MCP control and status)
- `Settings/` (settings model/store)
- `Utilities/` (logging + icon factory)
- `UI/SettingsWindow.xaml` (WPF settings dialog)
- `RevitPublicMCPBridge.addin` (manifest template)

## Prerequisites

- Revit 2027 installed
- Official Autodesk Revit Public MCP Server Tech Preview installed
- .NET SDK 10.0+ (for local build)
- Visual Studio 2022/2026 with .NET desktop tooling

## Build

From the project root:

```powershell
dotnet build .\RevitPublicMCPBridge.csproj -c Release
```

Output DLL:

- `bin\Release\net10.0-windows\RevitPublicMCPBridge.dll`

## Install in Revit 2027

1. Build the project.
2. Copy `RevitPublicMCPBridge.dll` (and any satellite output if generated) to a stable folder, for example:
   - `C:\ProgramData\Autodesk\Revit\Addins\2027\RevitPublicMCPBridge\`
3. Copy `RevitPublicMCPBridge.addin` to:
   - `C:\ProgramData\Autodesk\Revit\Addins\2027\`
4. Edit the `<Assembly>` path in `RevitPublicMCPBridge.addin` so it points to your deployed DLL.
5. Start Revit 2027 and verify the `MCP` ribbon tab appears.

## First-Run Setup

1. Click `Settings`.
2. Confirm:
   - Port (default `8765`)
   - Optional executable override path for `Autodesk.RevitMcpServer.Stdio.exe`
   - Optional arguments
3. Save.
4. Click `Start Server`.
5. Click `Status` and confirm endpoint like:
   - `ws://localhost:8765`
6. Click `Copy Config` to copy quick MCP connection details for Cursor/Claude.

## Notes

- If reflection binding is available in your installed MCP package, the bridge uses it automatically.
- Otherwise process control is used and remains fully functional.
- `Stop` is conservative by default (only stops instances started by this bridge). Enable `Allow stop any instance` in settings to force-stop all matching MCP processes.
- `Help / Docs` URL is configurable in settings.

## Troubleshooting

- **No button/tab appears:** verify `.addin` path and `<Assembly>` absolute path.
- **Start fails:** set explicit server executable path in `Settings`.
- **Status says running but no connection:** verify chosen port is not blocked or already in use.
- **Clipboard copy fails:** this can happen under restricted desktop/session states; the dialog displays the full config text for manual copy.
