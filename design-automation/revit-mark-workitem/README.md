# Revit Mark Workitem (Design Automation stub)

Starter bundle for applying **cached CONTROL_MARK proposals** to the **central** Revit model in APS Design Automation, then **Synchronize with Central** (TODO: wire per your ACC + worksharing setup).

`MCPToolHelper` and production tolerances live outside this repo—reference or package that logic into this add-in before production use.

## Layout

- `Source/MarkWorkitemApp.cs` — entry stub: read JSON payload, locate elements by **External ID** (must match AEC `External ID` / Viewer `externalId`), set `CONTROL_MARK`, optional SWC.
- `RevitMarkWorkitem.csproj` — targets **.NET Framework 4.8** + Revit API (adjust `RevitInstallPath` for your engine, e.g. DA **Revit 2024**).

## Build

1. Install matching **Revit** or copy `RevitAPI.dll` / `RevitAPIUI.dll` from the DA engine image.
2. Set `RevitInstallPath` in the `.csproj` or define `REVIT_API_PATH` and use `Directory.Build.props`.
3. `dotnet build -c Release`
4. Zip for AppBundle: `RevitMarkWorkitem.dll`, dependencies, `PackageContents.xml` (create per [DA Revit bundle](https://aps.autodesk.com/en/docs/design-automation/v3/developers_guide/revit/) docs).

## Workitem arguments (from web/MCP)

The Next.js and MCP clients POST a workitem whose `arguments.payload.text` is JSON aligned with `aec_analyze_v1` from [`aps-ai-web/src/lib/aec-elements-for-marks.ts`](../../aps-ai-web/src/lib/aec-elements-for-marks.ts):

- `version`, `intent`, `idResolution`, `marks[]` with `control_mark`, `externalIds`, `aecElementIds`
- `cache_id`, `provenance`

Your **Activity** must declare an input argument named `payload` (zip / JSON) matching what `da-workitems.ts` sends.

## Hybrid flow

1. **Cloud read**: AEC GraphQL + Viewer context → cache.
2. **DA write**: Open **central** RVT from OSS → apply marks → `SynchronizeWithCentral` (TODO: credentials, comment sets, errors).
3. **Desktop**: Users sync and republish to refresh the Viewer.

## Risks

- SWC in automation requires valid central paths and permissions; many teams use **create new version** without SWC in DA and rely on desktop sync—choose explicitly.
