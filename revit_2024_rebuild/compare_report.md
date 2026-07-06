# 2025 → 2024 Rebuild — 1:1 Takeoff Compare
_Source: `26-0349 A101 (2026 07-01) - MODEL` (2025) vs target `MODEL_R24` (2024). Generated 2026-07-05._

## Product-type quantity compare (real products; auto-nested DO-NOT-USE helpers excluded)

| Family | Source | Placed | Status |
|---|---:|---:|---|
| COLUMN (CLA) | 229 | 229 | ✅ |
| T_GIRDER (TGA) | 186 | 186 | ✅ |
| L_GIRDER (LGA) | 7 | 7 | ✅ |
| R_BEAM (RBA) | 7 | 7 | ✅ |
| DOUBLE_TEE_CHAMFERED_NOMINAL (DT) | 679 | 679 | ✅ (sloped) |
| INSULATED_WALL_PANEL (VERT) (WPBV) | 262 | 262 | ✅ |
| SOLID_WALL_PANEL (HORIZ) (WPAH) | 30 | 30 | ✅ |
| SOLID_WALL_PANEL (VERT) (WPAV) | 10 | 10 | ✅ |
| SHEARWALL_HORIZONTAL (SWAH) | 57 | 57 | ✅ |
| FLAT_SLAB (FSA) | 5 | 5 | ✅ |
| STAIR_LANDING (STF) | 9 | 9 | ✅ |
| STAIR_DROP_IN (STA) | 8 | 8 | ✅ |
| REVEAL | 2333 | 2333 | ✅ (cut) |
| CORBEL Light Loaded 12D X 24W X 36T | 54 | 54 | ⚠️ placeholder family |
| CORBEL Light Loaded (Double) 12D X 24W X 36T | 169 | 169 | ⚠️ placeholder family |
| VOID_RECTANGULAR (DOES NOT EXPORT) | 20 | 19 | ⚠️ 1 orphan slot not placed |
| Personal Doors | 7 | 7 | ✅ (cut) |
| Floor | 1 | 0 | ❌ boundary not exported |
| **TOTAL** | **4073** | **4071** | 2 deltas |

## Reveal takeoff (linear length by type)

| Reveal type (½" deep) | Count | Total lin ft | Avg len ft |
|---|---:|---:|---:|
| 1/2" x 1" | 512 | 11,968 | 23.4 |
| 1/2" x 3" | 262 | 3,041 | 11.6 |
| 1/2" x 4" | 262 | 3,039 | 11.6 |
| 1/2" x 6" | 1035 | 27,566 | 26.6 |
| 1/2" x 3'6" RECESS | 262 | 3,186 | 12.2 |
| **TOTAL** | **2333** | **48,800** | |

## Double-tee
679 × `12 DT 37 (3" FLANGE)`, lengths 21.2–81.1 ft, **47,189 lin ft** total. Sloped on 2 shared reference planes (~0.96°/1.15° gable).

## Deltas / flags — status after 2026-07-05 delta pass
1. **Void 10074904 — RESOLVED ✅** — was mis-classified: its "host" `10027565` is actually level **ROOF HIGH POINT**, so it was level-hosted (pass 1) and never cut anything. Re-hosted onto tower panel `7322731` (src `10061860`), corrected orientation (`facing=+Z/hand=+X` → face normal +Y), and swapped `Length`↔`Depth` (family profile = Width×Length, extrude Depth). Placed as new void `7347341` = **10'11" × 7'0" opening**, cut verified (face-reference). Voids now **19/20**.
2. **Void 10079126 — ORPHAN, not placed** — 14.8 m × 485 × 203 mm slot, `facing≈+Z`, **no host at all** in source. Spans far more than any single element; a phantom/artifact. Left out (invisible; cuts nothing). Confirm with source if a real slot is intended.
3. **Floor (1) — BLOCKED by export gap** — `Floor` `10080471` (`CONCRETE SLAB - 6"`, LEVEL 1) has **no boundary/sketch in the export** (empty placement). Cannot recreate without re-exporting its sketch profile from the 2025 source. Same root cause as the missing view/dimension data.
4. **Corbels (223) — placeholder family** — count is 1:1 but placed on placeholder `CORBEL 8D X 12W X 1.33T` (source `12D X 24W X 36T` single+double not in library). Geometry approximate; needs the real corbel family for takeoff-accurate geometry. (Plan only — deferred per user.)

**Net: 4071/4073 placed (99.95%). Remaining 2 = source-export-data gaps (floor sketch, orphan slot), not rebuild errors.**

## Views / sheets / details / dimensions — NOT yet reproducible
Source export has inventory only (14 sheets, 150 views by type/name/scale/template, viewport map, title block `TB_ERECTION_MC_30-42`) but **NO view content, crop regions, dimensions, detail-view content, or schedule definitions** — the export bridge lacked a view-detail tool. Must re-export these from the 2025 source before rebuilding sheets/details/dimensions.
