# Design Automation Revit workitem payload (execution dispatcher)

The Revit add-in entry **`RunRevitAutomationApp`** (MCP Tool **run_revit_automation**; legacy alias **`MarkWorkitemApp`**) reads JSON from **`MARK_PAYLOAD_JSON`** (or `%TEMP%\mark-payload.json` locally) and dispatches by **`operation`** and **`skip_analysis`**.

## Operations

| `operation` | Behavior |
|-------------|----------|
| `modify_parameters` | Applies `parameter_updates`, `parameterPatches`, and/or expanded `cached_selection` + `updates`. Never runs `marks[]` unless combined via legacy `apply_marks_and_modify`. |
| `run_mark_analysis` | Mark pass: `marks[]` → `CONTROL_MARK` via External ID (same as legacy `apply_marks`). |
| `apply_marks` | Legacy alias of the mark pass. |
| `apply_marks_and_modify` | Mark pass then parameter updates/patches. |
| `clear_cache` | No model transaction; audit-only stub. |

If `operation` is omitted, it is inferred from `skip_analysis` and whether `marks[]` is non-empty. When **`skip_analysis` is true**, the resolved operation is always **`modify_parameters`** (marks are never applied).

## Generic edits: `cached_selection` + `updates`

The add-in expands these into canonical `parameter_updates` rows:

```json
{
  "operation": "modify_parameters",
  "skip_analysis": true,
  "cached_selection": {
    "cache_id": "optional-uuid",
    "externalIds": ["id-1", "id-2"],
    "aecElementIds": [],
    "provenance": { "analyzedAt": "2026-01-01T00:00:00.000Z" }
  },
  "updates": [
    { "paramName": "CONTROL_MARK", "action": "clear" },
    { "paramName": "UNIFORMAT_CODE", "action": "set", "value": "X" },
    { "paramName": "MY_FLAG", "action": "toggle" }
  ]
}
```

- **Clear** uses `Parameter.ClearValue()` when possible, with type-specific fallback.
- Element resolution uses the Revit parameter **External ID** (Forge/AECDM `externalId`).

## Structured rows (alternative)

`parameter_updates[]` with `{ "externalIds": [], "paramName", "action", "value?" }`, plus optional `parameterPatches` / `additional_updates.parameterPatches`.

## Audit output (workitem artifacts)

The same JSON is written to:

1. **`audit_report.json`** — canonical path for DA output zipping: `{DA_ARTIFACTS_DIR}/audit_report.json`, or `%TEMP%/audit_report.json` if `DA_ARTIFACTS_DIR` is unset. Override file path with **`DA_AUDIT_REPORT_JSON`**.
2. **Legacy:** **`MARK_AUDIT_JSON`** if set, else `%TEMP%/mark-audit.json`.

Schema highlights (`revit_automation_audit_v2`):

- **`log[]`** — timestamped `level` + `text` lines suitable for Alice / web summaries.
- **`validation.cached_selection_warnings`** / **`validation.edit_target_warnings`** — stubs for future **validate_edit_target** (Tool B).
- **`summary`** — e.g. `unique_elements_resolved_modify`, aggregated modify ok/fail counts.
- **`modify`**, **`marks`**, **`patches`**, **`swc`**, **`post_run`** — execution details; **`post_run`** is the **post_run_verify** (Tool D) scaffold.

Paths used are echoed under **`artifact_paths`** in the JSON.

## Environment variables

| Variable | Purpose |
|----------|---------|
| `MARK_PAYLOAD_JSON` | Path to input JSON |
| `DA_ARTIFACTS_DIR` | Folder for `audit_report.json` (zip as Design Automation output) |
| `DA_AUDIT_REPORT_JSON` | Full path override for the canonical audit file |
| `MARK_AUDIT_JSON` | Legacy second copy path |
| `MARK_SWC` | `true` → `Document.SynchronizeWithCentral` after commit |
