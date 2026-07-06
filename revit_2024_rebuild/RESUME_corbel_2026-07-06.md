# RESUME NOTE — 2026-07-06 (corbel geometry rebuild)

## Why we paused
Switching Claude Code auth from subscription → `ANTHROPIC_API_KEY` (subscription limit). Relaunch with `claude --continue` to resume this conversation.

## Current task
Option (b) "wrap up the geometry" = replace the **placeholder** corbels (223) with the **real** source geometry. Everything else in the takeoff is 1:1 (4071/4073; the 2 remaining deltas are source-export gaps — floor sketch + one orphan slot — see `compare_report.md`).

## SOURCE CORBEL GEOMETRY — fully determined (read from 2025 model, bridge on port 8767)
Source model `26-0349 A101 (2026 07-01) - MODEL` is open in Revit 2025. Corbel families are OST_GenericModel.

**Single** (`CORBEL Light Loaded 12D X 24W X 36T`, src id 10032222, Vol = 141,584,233 mm³ = 5.00 CF):
- A simple **EXTRUSION** of a quadrilateral profile, extruded **609.6 mm (24") along the width (Y)**.
- Profile in the projection×height plane (X=proj, Z=height), vertices (X off 0..304.8mm, Z abs):
  - top edge flat, full 12" (304.8mm) wide at Z=top
  - **column face = full 36" (914.4mm) height** (one vertical side)
  - **free/outer end = 24" (609.6mm) height**
  - bottom is a **45° haunch chamfer** removing a 12"×12" (304.8×304.8mm) triangle from the bottom-outer corner
- Shoelace profile area 232,257 mm² × 609.6 = 141.58e6 mm³ ✓ = 5.00 CF.
- Material PRECAST CONCRETE. WorkPlaneBased, hosted on `COLUMN (CLA) : 30" X 30"`. facing=+Z, hand=-Y.

**Double** (`CORBEL Light Loaded (Double) 12D X 24W X 36T`, src id 10029072, Vol = 10.00 CF = 2 solids):
- **Two identical single corbels, mirrored**, full-height (column) faces facing each other, separated by **COLUMN THICKNESS** (762mm=30" for 140 of them, 609.6mm=24" for 29), projecting outward in opposite ±X directions. Total span = proj + col + proj (e.g. 12+30+12 = 54").

## IMMEDIATE NEXT STEP
1. (Optional, user asked) finish inspecting construction: the family editor for the single was opened via `open_family_editor_by_element_id(10032222)` but `get_family_document_info` returned "active doc is not a family" (activation lag). Re-open/activate the family editor in the 2025 bridge and read `get_family_document_info` + `get_family_parameters` to confirm it's an extrusion & see param names. (Geometry is already fully known, so this is confirmation only — a rigid extrusion reproduces it exactly. CORBEL_ADJUSTABLE.rfa is the production family but overkill for the estimating model.)
2. Build the real corbel in **2024**: author `create_extrusion` family(ies) with the profile above (single; double = two mirrored extrusions with a COLUMN THICKNESS gap param/type). Name to match source. Load into the 2024 rebuild model.
3. Re-place the 223 corbels on the real family (swap placeholder), preserving 1:1 count. Placeholder family = `CORBEL 8D X 12W X {T}` (params Depth_Overall/Width/Bearing/Depth_Haunch). Datasets: `instances_corbels.json` (223), `typemap_corbels.json`, `remap_corbels.json`. Single src example 10032222→placed 7329678; double src 10029072→placed 7329650.

## STATE / REMINDERS
- **SAVE the 2024 model** — new void `7347341` (tower opening, delta pass) placed but may be unsaved.
- Both bridges reconnect on relaunch: 2024 = port 8764, 2025 = port 8767. `.mcp.json` now includes both.
- 2025 family editor open is read-only; safe.
