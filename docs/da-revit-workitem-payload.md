# Design Automation Revit workitem payload (execution dispatcher)

The Revit add-in `MarkWorkitemApp` reads JSON from `MARK_PAYLOAD_JSON` (or `%TEMP%\mark-payload.json` locally) and dispatches by **`operation`** and **`skip_analysis`**.

## Operations

| `operation` | Behavior |
|---------------|----------|
| `modify_parameters` | Applies `parameter_updates`, `parameterPatches` (and/or expanded `cached_selection` + `updates`). No `marks[]` when `skip_analysis` is true. |
| `run_mark_analysis` | Same mark pass as legacy `apply_marks`: `marks[]` → `CONTROL_MARK` via External ID. |
| `apply_marks` | Legacy alias of the mark pass. |
| `apply_marks_and_modify` | Mark pass then parameter updates/patches. |
| `clear_cache` | No model transaction; audit-only stub for future hooks. |

If `operation` is omitted, it is inferred from `skip_analysis` and whether `marks[]` is non-empty. When **`skip_analysis` is true**, the resolved operation is always **`modify_parameters`** (marks are never applied).

## Generic edits: `cached_selection` + `updates`

The add-in expands these into canonical `parameter_updates` rows (the Next.js/MCP clients also expand for clearer workitem logging):

```json
{
  "operation": "modify_parameters",
  "skip_analysis": true,
  "cached_selection": {
    "cache_id": "optional-uuid",
    "externalIds": ["id-1", "id-2"],
    "aecElementIds": [],
    "provenance": {}
  },
  "updates": [
    { "paramName": "CONTROL_MARK", "action": "clear" },
    { "paramName": "UNIFORMAT_CODE", "action": "set", "value": "X" },
    { "paramName": "MY_FLAG", "action": "toggle" }
  ]
}
```

Element resolution uses the Revit parameter **External ID** (Forge/AECDM `externalId`).

## Structured rows (alternative)

Same as before: `parameter_updates[]` with `{ "externalIds": [], "paramName", "action", "value?" }`, plus optional `parameterPatches` / `additional_updates.parameterPatches`.

## Audit output

After a successful transaction, the add-in writes **`MARK_AUDIT_JSON`** (default `%TEMP%\mark-audit.json`) with:

- `marks.*` — groups processed, match/miss counts, optional `misses[]`
- `modify.*` — parameter update stats, `misses[]`, `failures[]` (read-only, missing param, exceptions)
- `patches.*` — patch row stats
- `swc` — optional **Synchronize With Central** attempt when `MARK_SWC=true` (otherwise skipped)

## Environment variables

| Variable | Purpose |
|----------|---------|
| `MARK_PAYLOAD_JSON` | Path to input JSON |
| `MARK_AUDIT_JSON` | Path for audit report |
| `MARK_SWC` | Set to `true` to call `Document.SynchronizeWithCentral` after commit (many teams leave this off in DA) |
