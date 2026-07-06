# Revit 2025 MCP Bridge — Groups 6–11 Spec

> Last updated: 2026-06-05
> Status: Group 6 built (v13, pending validation). Groups 7–11 specced, not yet built.
> Note: Group 8 has been extended to include sheet read/audit tools in addition to creation tools.

---

## Group 6 — Model Element Creation (v13, pending validation)

All tools: `POST /api/project/...`, guard `!doc.IsFamilyDocument`, wrap in Transaction, all spatial inputs mm.

| Tool | Route |
|---|---|
| `create_wall` | `/api/project/create_wall` |
| `create_floor` | `/api/project/create_floor` |
| `create_room` | `/api/project/create_room` |
| `create_structural_column` | `/api/project/create_structural_column` |
| `create_beam` | `/api/project/create_beam` |
| `create_point_based_element` | `/api/project/create_point_based_element` |
| `create_opening_by_boundary` | `/api/project/create_opening_by_boundary` |

---

## Group 7 — Modify and Edit

All tools: `POST /api/project/...`, 30s timeout for geometry ops, 10s for parameter ops.

### `move_elements`
- Route: `POST /api/project/move_elements`
- Request: `int[] ElementIds`, `double DeltaX, DeltaY, DeltaZ` (mm)
- Implementation: `ElementTransformUtils.MoveElement(doc, id, translation)`
- Returns: `{ movedCount, elementIds }`

### `rotate_elements`
- Route: `POST /api/project/rotate_elements`
- Request: `int[] ElementIds`, `double OriginX, OriginY, OriginZ` (mm), `double AngleDeg`, `string Axis` ("X"/"Y"/"Z")
- Implementation: `ElementTransformUtils.RotateElement(doc, id, axis, angleRad)`
- Returns: `{ rotatedCount, elementIds }`

### `mirror_elements`
- Route: `POST /api/project/mirror_elements`
- Request: `int[] ElementIds`, `double PlaneOriginX, PlaneOriginY, PlaneOriginZ` (mm), `string PlaneNormal` ("X"/"Y"/"Z"), `bool CreateCopy` (default true)
- Implementation: `ElementTransformUtils.MirrorElement(doc, id, plane)`
- Returns: `{ mirroredCount, elementIds }`

### `copy_elements_to_level`
- Route: `POST /api/project/copy_elements_to_level`
- Request: `int[] ElementIds`, `int TargetLevelId`
- Implementation: `ElementTransformUtils.CopyElements(doc, ids, targetDoc, transform, options)`
- Returns: `{ copiedCount, newElementIds }`

### `set_element_parameters`
- Route: `POST /api/project/set_element_parameters`
- Request: `int ElementId`, `object Parameters` (key/value pairs)
- Implementation: iterate parameters, match by name, set value with type-aware conversion
- Returns: `{ setCount, skipped: string[] }`

### `delete_elements`
- Route: `POST /api/project/delete_elements`
- Request: `int[] ElementIds`
- Implementation: `doc.Delete(ids)` inside transaction — returns list of actually deleted IDs (Revit may cascade-delete dependents)
- Returns: `{ deletedCount, deletedIds }`

---

## Group 8 — Views, Sheets and Sheet Audit ← EXTENDED

### Creation tools

#### `create_floor_plan_view`
- Route: `POST /api/project/create_floor_plan_view`
- Request: `int LevelId`, `string? Name`
- Implementation: `ViewPlan.Create(doc, viewFamilyTypeId, levelId)` where viewFamilyTypeId is the first Floor Plan ViewFamilyType
- Returns: `{ id, name, levelName }`

#### `create_reflected_ceiling_plan`
- Route: `POST /api/project/create_reflected_ceiling_plan`
- Request: `int LevelId`, `string? Name`
- Implementation: same as floor plan but filter for Ceiling Plan ViewFamilyType
- Returns: `{ id, name, levelName }`

#### `create_section_view`
- Route: `POST /api/project/create_section_view`
- Request: `double[] BoundingBoxMin` (mm), `double[] BoundingBoxMax` (mm), `string? Name`
- Implementation: `ViewSection.CreateSection(doc, viewFamilyTypeId, transform)` — transform derived from bbox
- Returns: `{ id, name }`

#### `create_3d_view`
- Route: `POST /api/project/create_3d_view`
- Request: `string? Name`, `bool IsOrthographic` (default false)
- Implementation: `View3D.CreateIsometric` or `CreatePerspective`
- Returns: `{ id, name }`

#### `create_sheet`
- Route: `POST /api/project/create_sheet`
- Request: `string SheetNumber`, `string SheetName`, `int? TitleBlockTypeId`
- Implementation: `ViewSheet.Create(doc, titleBlockTypeId)` → set sheet number and name parameters
- Returns: `{ id, sheetNumber, sheetName, titleBlockName }`

#### `place_view_on_sheet`
- Route: `POST /api/project/place_view_on_sheet`
- Request: `int SheetId`, `int ViewId`, `double LocationX, LocationY` (mm — center point on sheet)
- Implementation: `Viewport.Create(doc, sheetId, viewId, point)`
- Returns: `{ viewportId, sheetNumber, viewName }`

#### `set_view_crop_region`
- Route: `POST /api/project/set_view_crop_region`
- Request: `int ViewId`, `double MinX, MinY, MaxX, MaxY` (mm)
- Implementation: set `view.CropBox` BoundingBoxXYZ, set `view.CropBoxActive = true`
- Returns: `{ viewId, cropBoxMm }`

---

### Read and Audit tools

#### `get_sheets`
- Route: `POST /api/project/get_sheets`
- Description: List all sheets in the project
- Request: none
- Implementation: `FilteredElementCollector` → `ViewSheet` class → for each return id, sheetNumber, sheetName, title block name, list of viewport IDs placed on sheet
- Returns: `{ sheets: Array<{ id, sheetNumber, sheetName, titleBlock, viewportIds }>, totalCount }`
- Timeout: 10s

#### `get_sheet_contents`
- Route: `POST /api/project/get_sheet_contents`
- Description: Full inventory of everything on a sheet — viewports, annotations, text, title block fields
- Request: `int SheetId`
- Implementation:
  - Get `ViewSheet` by ID
  - Collect all `Viewport` elements on sheet → return viewId, viewName, viewType, center point (mm), crop box (mm)
  - Collect all `TextNote` elements → return text content, position (mm), font size
  - Collect all `IndependentTag` elements → return taggedElementId, position (mm)
  - Collect title block instance → return all filled parameter values (sheet number, name, date, drawn by, checked by, revision etc.)
  - Collect `FilledRegion` elements → return count and positions
- Returns:
```typescript
{
  sheetId: number,
  sheetNumber: string,
  sheetName: string,
  titleBlockParameters: { [key: string]: string },
  viewports: Array<{ viewId, viewName, viewType, centerMm, cropBoxMm }>,
  textNotes: Array<{ text, positionMm, fontSize }>,
  tags: Array<{ taggedElementId, positionMm }>,
  filledRegions: Array<{ id, positionMm }>,
  totalAnnotationCount: number
}
```
- Timeout: 10s

#### `open_sheet`
- Route: `POST /api/project/open_sheet`
- Description: Set the active Revit view to a specific sheet by sheet number or ID
- Request: `string? SheetNumber`, `int? SheetId` (one required)
- Implementation: find `ViewSheet` by number or ID → `uiDoc.ActiveView = sheet`
- Returns: `{ id, sheetNumber, sheetName }`
- Timeout: 10s

#### `get_view_contents`
- Route: `POST /api/project/get_view_contents`
- Description: Inventory of everything inside a view — elements, tags, dimensions, annotations
- Request: `int ViewId`, `bool IncludeElements` (default true), `bool IncludeAnnotations` (default true)
- Implementation:
  - `FilteredElementCollector(doc, viewId)` for model elements → group by category, return counts and IDs
  - Collect `Dimension` elements in view → return count, labeled vs unlabeled
  - Collect `IndependentTag` elements → return taggedElementIds, identify untagged elements by comparing to model elements
  - Collect `TextNote` elements
  - Identify model elements in view with no tag → return as `untaggedElements` array
- Returns:
```typescript
{
  viewId: number,
  viewName: string,
  viewType: string,
  elementsByCategory: { [category: string]: { count: number, ids: number[] } },
  dimensions: { total: number, labeled: number, unlabeled: number },
  tags: { total: number, taggedElementIds: number[] },
  untaggedElements: Array<{ id, category, familyName }>,
  textNotes: Array<{ text, positionMm }>,
  totalElementCount: number
}
```
- Timeout: 30s

#### `compare_sheet_to_template`
- Route: `POST /api/project/compare_sheet_to_template`
- Description: Diff a sheet against a JSON template definition. Returns missing elements, unpopulated title block fields, missing views, misplaced viewports.
- Request:
```csharp
int SheetId
string TemplateJson   // serialized SheetTemplate object (see schema below)
```
- Template JSON schema:
```typescript
{
  requiredTitleBlockFields: string[],        // parameter names that must be non-empty
  requiredViewTypes: string[],               // view type names that must be placed (e.g. "Floor Plan", "Section")
  requiredViewCount: number,                 // minimum number of viewports
  requiredAnnotations: {
    minimumDimensions: number,
    requireAllElementsTagged: boolean,
    requiredTextNotes: string[]              // text strings that must appear somewhere on sheet
  }
}
```
- Implementation: call `get_sheet_contents` internally → diff against template → build issues list
- Returns:
```typescript
{
  sheetId: number,
  sheetNumber: string,
  passed: boolean,
  issues: Array<{
    severity: "error" | "warning",
    category: "titleBlock" | "viewports" | "annotations" | "tags" | "text",
    description: string,
    elementId?: number
  }>,
  issueCount: number
}
```
- Timeout: 30s

---

## Group 9 — Grids and Levels

### `create_level`
- Route: `POST /api/project/create_level`
- Request: `double ElevationMm`, `string Name`
- Implementation: `Level.Create(doc, elevationFeet)`  → set name
- Returns: `{ id, name, elevationMm }`

### `set_level_elevation`
- Route: `POST /api/project/set_level_elevation`
- Request: `int LevelId`, `double ElevationMm`
- Implementation: `level.Elevation = mm / 304.8`
- Returns: `{ id, name, elevationMm }`

### `create_grid`
- Route: `POST /api/project/create_grid`
- Request: `double StartX, StartY, EndX, EndY` (mm, Z=0), `string? Name`
- Implementation: `Grid.Create(doc, Line.CreateBound(start, end))` → set name
- Returns: `{ id, name, startMm, endMm }`

### `create_grid_arc`
- Route: `POST /api/project/create_grid_arc`
- Request: `double CenterX, CenterY` (mm), `double RadiusMm`, `double StartAngleDeg`, `double EndAngleDeg`, `string? Name`
- Implementation: `Grid.Create(doc, Arc.Create(center, radius, startAngle, endAngle, XYZ.BasisX, XYZ.BasisY))`
- Returns: `{ id, name }`

---

## Group 10 — Annotation

### `create_project_dimension`
- Route: `POST /api/project/create_project_dimension`
- Request: `int ViewId`, `int[] ReferenceIds`, `double LineStartX, LineStartY, LineEndX, LineEndY` (mm)
- Implementation: `doc.Create.NewDimension(view, line, referenceArray)`
- Returns: `{ id, valueMm }`

### `tag_element`
- Route: `POST /api/project/tag_element`
- Request: `int ViewId`, `int ElementId`, `double LocationX, LocationY` (mm), `bool HasLeader` (default false)
- Implementation: `IndependentTag.Create(doc, viewId, reference, hasLeader, TagMode.TM_ADDBY_ELEMENT, TagOrientation.Horizontal, point)`
- Returns: `{ id, taggedElementId }`

### `add_text_note`
- Route: `POST /api/project/add_text_note`
- Request: `int ViewId`, `string Text`, `double LocationX, LocationY` (mm), `double? Width` (mm), `int? TextNoteTypeId`
- Implementation: `TextNote.Create(doc, viewId, point, width, text, options)`
- Returns: `{ id, text, positionMm }`

### `create_spot_elevation`
- Route: `POST /api/project/create_spot_elevation`
- Request: `int ViewId`, `int HostId`, `double PointX, PointY, PointZ` (mm), `double BendX, BendY` (mm), `double EndX, EndY` (mm)
- Implementation: `doc.Create.NewSpotElevation(view, reference, origin, bend, end, refPt, hasLeader)`
- Returns: `{ id }`

---

## Group 11 — Query and Utility (partial — v12)

### Live tools (v12)
- `get_levels` — all levels with elevation
- `get_grids` — all grids with endpoints
- `get_element_types` — loaded family types, filterable by category
- `query_elements` — general query by category/level/bbox
- `get_element_by_id` — full metadata + parameters for one element

### Still to add
- `get_rooms` — all rooms with name, number, level, area, location
- `get_sheets` — moved to Group 8 read/audit
- `get_views` — all views with type, associated level, crop box
- `undo_last_action` — `uiDoc.Document.Undo()` — verify UIApplication access before implementing
