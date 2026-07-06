# Engineering MCP — Consultant Project Brief
_Prepared for an external AI consultant (Grok). Self-contained; assumes no prior context._
_Status as of 2026-07-06: reconnaissance complete, no code written yet, awaiting go-ahead._

---

## 1. What this project is

A **standalone Engineering MCP server** that performs **structural engineering load takedowns** on Metromont precast-concrete Revit models. It is a fresh project that runs **in parallel** to an existing Revit "bridge" MCP.

**Hard architectural boundary:** the Engineering MCP has **NO Revit dependency**. It does not talk to Revit, has no HTTP gateway, and imports no Autodesk API. It consumes **JSON model snapshots** exported by the Revit bridge and does 100% of its work as **deterministic Python** (geometry + graph traversal + arithmetic). No estimation, no ML, no fuzzy logic — a load number is either derived from geometry or it is flagged as a gap.

**Why it exists:** to produce a preliminary **gravity load takedown** (dead load + roof live load accumulating down through the structure to the foundations) directly from the BIM geometry, as an internal coordination aid for the precast engineering team.

---

## 2. The larger ecosystem (context only)

- **Revit bridge MCPs** (`revit-2024-bridge-mcp`, `revit-2025-bridge-mcp`): Node/TypeScript MCP servers that talk over an HTTP gateway (ports 8764 / 8767) to a C# Revit add-in. They can read/query/create Revit elements and **export model snapshots** to JSON via an `export_instances` tool. These are the *producers* of our input.
- **Engineering MCP** (this project): the *consumer*. Python, stdio, offline.
- The two communicate **only** through a versioned JSON snapshot file on disk (the "v1 snapshot contract" — see §6).

The consultant does **not** need Revit knowledge — treat the input as "a JSON file of 3D concrete objects with geometry and parameters."

---

## 3. Domain context (this drives every design decision)

Metromont is a **precast concrete manufacturer**. Key facts that break normal BIM assumptions:

1. **Everything structural is one Revit category.** Columns, wall panels, T-girders, spandrels, and Metrodeck all live in `OST_StructuralFraming`. You cannot rely on category to distinguish a column from a beam — you must use geometry + family/type names + parameters.
2. **Elements routinely span multiple levels.** A wall panel or column may be 3+ stories tall. Therefore:
   - **Never** use "Reference Level" / level assignment to determine an element's vertical extent or bearing. **Geometry only.**
   - Every element's `zMin`/`zMax` comes from its **solid geometry / bounding box**.
   - A single element can have **multiple bearing interfaces at different elevations** (e.g., a tall panel receiving deck at L2, L3, and roof; a column with a mid-height corbel).
   - Levels are used **only as reporting bins** for accumulated loads — never as structural logic.
3. **Precast roof deck (Metrodeck, double-tees) spans ONE-WAY** to bearing lines. One-way span load distribution is the default; generic polygon tributary area is the fallback for irregular conditions.
4. **Bearing is a contact-geometry relationship:** element A bears on element B when A's *bottom* horizontal face is within a vertical tolerance of B's *top* horizontal face **and** their XY footprints overlap by at least a minimum contact area. Loads flow **supported → supporting**, down to the lowest elevation (foundation).

---

## 4. Tech stack (decided — do not revisit)

- **Python 3.11+**, **FastMCP** stdio server (no HTTP, no gateway).
- **shapely** (polygon/footprint intersection math), **networkx** (directed bearing graph), **numpy**.
- **venv** inside the project folder.
- Registered in `claude_desktop_config.json` as `"engineering-mcp"` → the venv `python.exe` running the server entry script.
- Project location: `D:\CursorRevitMCP\engineering-mcp`.

---

## 5. Units convention

- **Input snapshot is in millimeters** (bridge convention).
- **Internal computation** stays in a single consistent system — proposed: **mm** for all geometry (shapely in mm / mm²), converting to Imperial only at the report boundary.
- **All report output is Imperial:** feet, kips, psf, pcf. Concrete density default **150 pcf**.
- Dead Load (DL) and Roof Live Load (RLL) are kept as **separate load cases end-to-end** (never summed until the report's Total column).

---

## 6. The v1 snapshot schema (bridge → engineering MCP contract)

Per element:
- `elementId`, `category`, `familyName`, `typeName`
- `boundingBox` (min/max XYZ, mm)
- `solidVolume` (mm³)
- `topFaces` and `bottomFaces` — horizontal face polygons, each = an XY vertex loop + a Z elevation (mm)
- `parameters` dict including `CONTROL_MARK`, `MANUFACTURE_COMPONENT`, `Assembly Name` where present

Plus a top-level `levels[]` array of `{ name, elevation (mm) }`.

This schema is written as a **JSON Schema file in the repo**, and **every snapshot load is validated against it** (strict; fail loudly).

---

## 7. ⚠️ BLOCKING FINDING — the current export does NOT satisfy the v1 contract

I inspected the only real export present (`instances.json`; model `26-0349 A101`; **5,831 elements**; categories `OST_StructuralFraming`, `OST_GenericModel`, `OST_Floors`).

**Present:** `elementId, uniqueId, category, familyName, typeName, typeId, levelName, levelId, placementType, placement{point xyz mm, rotationDeg}, orientation, flips, phase, hostinfo, parameters{}` (parameters include `CONTROL_MARK`).

**Missing (all required by v1):**
- `boundingBox` — absent (no z-extents)
- `solidVolume` (mm³) — absent as a field
- `topFaces` / `bottomFaces` — **absent entirely; no face geometry at all**
- top-level `levels[]` with elevations — absent (per-element `levelName` only)

**What exists but is NOT a valid substitute:** a `Volume` *parameter* (raw ≈ 203.255 **ft³**, Revit's internal unit — not mm³), plus `MEMBER_VOLUME_CAST`, `WEIGHT_PER_UNIT` (150). There are **zero face footprints and zero bounding boxes**, so **bearing detection is impossible from the current export**, and we will **not** approximate footprints from bounding boxes.

**The good news:** this geometry exists live in Revit and is retrievable today (bounding boxes, `Solid.Volume`, and planar faces are all readable via the bridge's `get_element_geometry`). The bridge simply doesn't serialize them in `export_instances`. **Required bridge-side extension (open item #1):**
- `boundingBox` ← `element.get_BoundingBox(null)` → Min/Max (mm)
- `solidVolume` ← Σ `Solid.Volume` (mm³)
- `topFaces`/`bottomFaces` ← iterate `Solid.Faces`, keep `PlanarFace` where `|normal.Z| ≈ 1`, emit `EdgeLoops` as XY loops + Z (mm)
- `levels[]` ← document levels → `{name, Elevation}` (mm). (This data exists today in a sibling file `levels_grids.json`: ROOF 8534.4 mm, ROOF HIGH POINT 10210.8, T.O. PARAPET 10680.7, DUNNAGE 14655.8, T.O. ELEV TOWER 20472.4; LEVEL 1 ≈ 0.)

Also: a referenced validation model, **EDCAUS31**, is **not present** in the working folder; it would also require the extended export.

---

## 8. V1 toolset (in build order)

1. **`load_model_snapshot(path)`** — validate against JSON Schema, build indexes, return stats: element counts by category, elevation range, level list, count of elements spanning >1 level.
2. **`get_vertical_extents(elementIds?)`** — zMin/zMax/height per element from geometry, plus which levels each element passes through (informational only).
3. **`build_bearing_graph(verticalToleranceMm=25, minOverlapAreaMm2)`** — detect bearing interfaces: A's bottom face within tolerance of B's top face **and** XY footprint intersection ≥ threshold (shapely). Output a **directed graph** (loads flow supported → supporting). **Must support mid-height bearing (corbels)** — compare all horizontal faces, not just extreme zMin/zMax. Diagnostics: unsupported elements (no path to lowest elevation), cycles (must be none — flag as errors), interface list with elevation + contact area.
4. **`assign_roof_loads(superimposedDeadPsf, roofLivePsf, spanMode='one-way')`** — identify roof-level deck elements, distribute area loads to bearing supports per span mode. DL and RLL kept separate.
5. **`run_gravity_takedown(concreteDensityPcf=150)`** — self-weight per element from `solidVolume × density`; walk the bearing graph top-down, accumulating DL (self-weight + superimposed) and RLL separately per element and per interface. Pure traversal + arithmetic.
6. **`generate_takedown_report(outputPath)`** — markdown + machine-readable JSON. Broken out by: per-element loads, per bearing interface binned by nearest level, grand totals. Columns: Dead, Roof Live, Total. Imperial. **Diagnostics section prominent** (unsupported elements, zero-volume elements, orphan roof areas) — "a takedown with silent gaps is worse than no takedown."

Every report carries a footer: **_preliminary load takedown for internal coordination; not for construction; requires PE review._**

---

## 9. Build discipline

- **Vertical slice first.** Before tools 4–6, get tools 1–3 correct against a **hand-built fixture snapshot** in `tests/fixtures/` — a minimal 3-element case (deck panel → beam → multi-level wall panel, including a **mid-height interface**) with hand-calculated expected answers. Every geometry function gets a pytest against known answers. Do not proceed to takedown logic until the fixture bearing graph is exactly correct.
- Then validate tool 3 against a **real snapshot subset** (small spatial region) before full-model — **currently BLOCKED** pending the bridge export extension.
- Maintain `D:\CursorRevitMCP\engineering_mcp_scope.json` (version, tool inventory, open items, and a `ccReorientationPrompt` for future sessions), following the existing `project_scope.json` pattern.
- If the real export lacks `solidVolume` or face footprints (**it does**), stop after tool 1 on real data, document the exact bridge additions, and keep them at the top of open items. Never work around missing geometry by approximating from bounding boxes.

---

## 10. Current status & immediate next steps

- **Done:** reconnaissance (bridge conventions, config, real export schema). The v1-contract gap is confirmed and characterized (§7).
- **Not started:** any code. Awaiting confirmation on two points:
  1. **Units:** geometry internal in mm, Imperial only at report boundary — OK?
  2. **Bridge ownership:** document the `export_instances` additions as a prerequisite; do **not** modify the TS bridge from within this project — correct?
- **Next once confirmed:** scaffold project (venv + deps + FastMCP stdio) → write v1 JSON Schema + strict validator → build & pytest tools 1–3 against the fixture → author `engineering_mcp_scope.json` with the bridge extension as open-item #1.

---

## 11. Open items / risks

1. **[PREREQUISITE] Bridge `export_instances` must emit geometry** (bbox, solidVolume mm³, top/bottom planar faces, levels[]). Everything past tool 1 on real models is blocked until this lands.
2. **EDCAUS31 validation model not available** in the working folder.
3. **One-way span direction inference** — how to determine span direction for deck (from geometry aspect ratio? explicit param? bearing-line adjacency?) is an open engineering question.
4. **Irregular tributary fallback** — when one-way doesn't apply, generic polygon tributary needs a defined method.
5. **Lateral / uplift / load combinations** — v1 is gravity-only (DL + RLL). ASCE 7 load combinations, wind/seismic, and uplift are explicitly out of v1 scope.

---

## 12. Where a consultant adds the most value

- Sanity-checking the **structural load-path methodology** (bearing detection tolerances, one-way vs tributary distribution, how to bin multi-level elements, how to treat corbels and mid-height interfaces).
- Advising on **span-direction inference** and **tributary fallback** rules from geometry alone.
- Reviewing the **diagnostics taxonomy** — what "gap" conditions must be surfaced so a preliminary takedown is trustworthy.
- Guidance on eventual **load combinations / lateral** scope (post-v1) without over-building v1.
- Independent review of the **hand-calculated fixture** expected values before they become the regression oracle.
