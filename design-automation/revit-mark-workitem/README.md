# Revit Mark Workitem (Design Automation stub)

Starter bundle for applying **cached CONTROL_MARK proposals** to the **central** Revit model in APS Design Automation, then **Synchronize with Central** (TODO: wire per your ACC + worksharing setup).

`MCPToolHelper` and production tolerances live outside this repo—reference or package that logic into this add-in before production use.

## Layout

- `Source/RunRevitAutomationApp.cs` — MCP Tool **run_revit_automation**: command dispatcher; **`RunRevitAutomationApp`** is the primary `IExternalCommand` (register this in new activities). **`MarkWorkitemApp`** in the same file delegates to the same logic for legacy bundle registrations.
- **`operation`**: `modify_parameters` (no mark side-effects), `run_mark_analysis` (and legacy `apply_marks` / `apply_marks_and_modify`), `clear_cache` (no-op audit). **`skip_analysis: true`** forces `modify_parameters` only.
- Audit: **`audit_report.json`** (under **`DA_ARTIFACTS_DIR`** or `%TEMP%`, or override with **`DA_AUDIT_REPORT_JSON`**), plus legacy **`MARK_AUDIT_JSON`** / `mark-audit.json`. Structured **`log[]`** for chat-oriented messages.
- Optional SWC: **`MARK_SWC=true`** → `Document.SynchronizeWithCentral`.
- `RevitMarkWorkitem.csproj` — year-aware build (2024 defaults to **net48**, 2025-2026 defaults to **net8.0-windows**, 2027+ defaults to **net10.0-windows**). Override with MSBuild properties when needed.

## Build & publish AppBundle

1. Install the matching Revit year locally (or set `REVIT_INSTALL_PATH` / `-RevitInstallPath` to the folder that contains `RevitAPI.dll`).
2. From this folder, run:
   - `.\build-appbundle.ps1` — default Revit 2024 (`net48`) output `RevitMarkWorkitem-AppBundle-2024.zip`.
   - `.\build-appbundle.ps1 -RevitYear 2025` — default `net8.0-windows`, output `RevitMarkWorkitem-AppBundle-2025.zip`.
   - `.\build-appbundle.ps1 -RevitYear 2027` — default `net10.0-windows`, output `RevitMarkWorkitem-AppBundle-2027.zip`.
   - `.\build-appbundle-matrix.ps1` — builds 2024/2025/2026/2027 zips in one run.
   - Scripts auto-load env vars from `aps-ai-web/.env.local` by default (override with `-EnvFile`).
3. Upload a **new version** to Design Automation (two-legged `code:all`):
   - Set `APS_CLIENT_ID`, `APS_CLIENT_SECRET`, and either:
     - `DA_APPBUNDLE_ID` (single bundle id), or
     - `DA_APPBUNDLE_ID_<YEAR>` (for year-specific bundle ids; e.g. `DA_APPBUNDLE_ID_2025`).
   - `.\publish-appbundle.ps1 -RevitYear 2025` — POST new version + multipart upload (defaults engine to `Autodesk.Revit+2025`).
4. (Recommended) publish activity version + alias in the same command:
   - Set `DA_ACTIVITY_ID` or `DA_ACTIVITY_ID_<YEAR>` (either can include `+alias`).
   - Optional alias override: `DA_ACTIVITY_ALIAS` (default `prod`).
   - Optional profile: `-ActivityProfile mark` (default) or `-ActivityProfile create_model`.
   - Script behavior:
     - Upload appbundle version
     - POST `/activities/{id}/versions` with new appbundle version
     - Create alias (or PATCH existing alias) to the new activity version
5. First-time bundle only: `.\publish-appbundle.ps1 -InitializeBundle -RevitYear 2025 -BundleId <new-id>` (or set `DA_APPBUNDLE_ID` / `DA_APPBUNDLE_ID_<YEAR>`).
6. First-time activity only: add `-InitializeActivity` with `-ActivityId <nickname.activityId>` (or env `DA_ACTIVITY_ID*`), then rerun normally for subsequent versions.

Use `-SkipActivityPublish` if you intentionally want upload-only behavior.

## Runtime routing in app

`aps-ai-web` now supports activity routing by Revit major version (`2024`, `2025+`, `2027+`) using:

- `DA_ACTIVITY_ID` (fallback)
- `DA_ACTIVITY_ID_2024`
- `DA_ACTIVITY_ID_2025`
- `DA_ACTIVITY_ID_2026`
- `DA_ACTIVITY_ID_2027`
- `DA_ACTIVITY_ID_NET8`
- `DA_ACTIVITY_ID_NET10`

Use API check endpoint to verify routing for a selected model version:

- `GET /api/aps/da-config-check?projectId=<dmProjectId>&versionId=<versionUrn>`

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
