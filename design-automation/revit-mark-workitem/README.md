# Revit Mark Workitem (Design Automation stub)

Starter bundle for applying **cached CONTROL_MARK proposals** to the **central** Revit model in APS Design Automation, then **Synchronize with Central** (TODO: wire per your ACC + worksharing setup).

`MCPToolHelper` and production tolerances live outside this repo—reference or package that logic into this add-in before production use.

## Layout

- `Source/RunRevitAutomationApp.cs` — MCP Tool **run_revit_automation**: command dispatcher; **`RunRevitAutomationApp`** is the primary `IExternalCommand` (register this in new activities). **`MarkWorkitemApp`** in the same file delegates to the same logic for legacy bundle registrations.
- **`operation`**: `modify_parameters` (no mark side-effects), `run_mark_analysis` (and legacy `apply_marks` / `apply_marks_and_modify`), `clear_cache` (no-op audit). **`skip_analysis: true`** forces `modify_parameters` only.
- Audit: **`audit_report.json`** (under **`DA_ARTIFACTS_DIR`** or `%TEMP%`, or override with **`DA_AUDIT_REPORT_JSON`**), plus legacy **`MARK_AUDIT_JSON`** / `mark-audit.json`. Structured **`log[]`** for chat-oriented messages.
- Optional SWC: **`MARK_SWC=true`** → `Document.SynchronizeWithCentral`.
- `RevitMarkWorkitem.csproj` — targets **.NET Framework 4.8** + Revit API (adjust `RevitInstallPath` for your engine, e.g. DA **Revit 2024**).

## Build & publish AppBundle

1. Install **Revit 2024** (or set `REVIT_INSTALL_PATH` / `-RevitInstallPath` to the folder that contains `RevitAPI.dll`).
2. From this folder, run:
   - `.\build-appbundle.ps1` — `dotnet build` + `RevitMarkWorkitem-AppBundle.zip` (`PackageContents.xml` + `Contents/*.dll`).
3. Upload a **new version** to Design Automation (two-legged `code:all`):
   - Set `APS_CLIENT_ID`, `APS_CLIENT_SECRET`, `DA_APPBUNDLE_ID` (your registered bundle id).
   - `.\publish-appbundle.ps1` — POST new version + multipart upload to signed URL (see [Revit publish tutorial](https://aps.autodesk.com/en/docs/design-automation/v3/tutorials/revit/step4-publish-appbundle/)).
4. First-time bundle only: `.\publish-appbundle.ps1 -InitializeBundle` with `DA_APPBUNDLE_ID` set to the **new** id and `DA_ENGINE` if not `Autodesk.Revit+2024`.
5. Bump your **Activity** alias (or `DA_ACTIVITY_ID`) to the new AppBundle version after upload.

Manual reference: [DA Revit developer guide](https://aps.autodesk.com/en/docs/design-automation/v3/developers_guide/revit/).

## Workitem arguments (from web/MCP)

The Next.js and MCP clients POST a workitem whose `arguments.payload.text` is JSON aligned with `aec_analyze_v1` from [`aps-ai-web/src/lib/aec-elements-for-marks.ts`](../../aps-ai-web/src/lib/aec-elements-for-marks.ts):

- `version`, `intent`, `idResolution`, `marks[]` with `control_mark`, `externalIds`, `aecElementIds`
- `cache_id`, `provenance`
- **Execution-only edits:** `operation`, `skip_analysis`, `parameter_updates[]`, `parameterPatches`, and/or `cached_selection` + `updates[]` (see [`../../docs/da-revit-workitem-payload.md`](../../docs/da-revit-workitem-payload.md) and `sample-payload-modify-parameters.json`)

Your **Activity** must declare an input argument named `payload` (zip / JSON) matching what `da-workitems.ts` sends.

## Hybrid flow

1. **Cloud read**: AEC GraphQL + Viewer context → cache.
2. **DA write**: Open **central** RVT from OSS → apply marks → `SynchronizeWithCentral` (TODO: credentials, comment sets, errors).
3. **Desktop**: Users sync and republish to refresh the Viewer.

## Risks

- SWC in automation requires valid central paths and permissions; many teams use **create new version** without SWC in DA and rely on desktop sync—choose explicitly.
