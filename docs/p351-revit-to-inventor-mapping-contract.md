# P351 Revit -> Inventor Mapping Contract (Manufacturing Parity)

## Scope

This contract defines manufacturing parity for `STEEL_PLATE 1-4 x 4 x Length`:

- Geometry-driving dimensions and hole/slot controls.
- Production-routing selections (CIP vs Erection, finish selectors).
- Output fields used for issuance/team routing (`MANUFACTURE_COMPONENT`, descriptions, marks, finish output fields).
- iLogic-heavy implementation is allowed to generate output strings from selector inputs.

## Extraction Evidence and Confidence

### Live extraction (high confidence)

- Revit instance: `processId=39944`
- Active doc: `STEEL_PLATE 1-4 x 4 x Length.rfa`
- Inventor target: `D:\Metromont Parts\STEEL_PLATE 1-4 x 4 x Length.ipt`
- Measured family geometry in active family context:
  - thickness: `0.0208333333 ft` (`0.25 in`)
  - width: `0.3333333333 ft` (`4 in`)
  - length: `1.0 ft` (`12 in`)

### Repository-derived parity hints (medium confidence)

- Existing mapping references and EDGE conventions indicate required identifiers and metadata behavior:
  - `MANUFACTURE_COMPONENT`
  - `CONTROL_MARK`
  - `IDENTITY_DESCRIPTION_SHORT`
  - `IDENTITY_DESCRIPTION`
  - finish-related fields (`FINISH_*`, ticketing finish fields)
  - CIP/Erection operational routing patterns.

### Limitation note

Current Revit public MCP endpoints expose model element data but not full Family Manager formula tables. Formula-level parity therefore uses:
- direct measured drivers from live family extraction, plus
- selector/output parity rules enforced in Inventor iLogic.

## Unit Normalization Rules

- Revit length doubles are feet.
- Inventor driving units for this part are inches.
- Conversion:
  - `in = ft * 12`
  - `mm = ft * 304.8` (optional mirror for manufacturing exports)

## Parameter Classes

### A) Geometry-driving inputs

- `DIM_LENGTH` -> `DIM_LENGTH` and `PlateLength` (in)
- `DIM_WIDTH` -> `PlateWidth` (in)
- `DIM_THICKNESS` (or equivalent plate depth) -> `PlateThickness` (in)

### B) Feature controls (hole/slot/void)

- Hole controls:
  - `HOLE_DIAMETER` -> `Plate_Hole_Diameter`
  - `HOLE_1..HOLE_4` -> `Plate_Hole1_Enabled..Plate_Hole4_Enabled`
  - `HOLE_1_edge..HOLE_4_edge` -> `Plate_Hole*_Edge`
  - `HOLE_1_side..HOLE_4_side` -> `Plate_Hole*_Side`
- Slot controls:
  - `SLOT_DIAMETER` -> `Plate_Slot_Diameter`
  - `SLOT_LENGTH` -> `Plate_Slot_Length`
  - `SLOT_1_VERT`, `SLOT_2_VERT`, `SLOT_1_HORIZ`, `SLOT_2_HORIZ` -> slot enable flags.

### C) Routing and finish selectors (selection parity required)

- Routing selectors (canonicalized in Inventor):
  - `Route_CIP` (bool)
  - `Route_Erection` (bool)
- Finish selectors (canonicalized in Inventor):
  - `Finish_Galvanized` (bool)
  - `Finish_PaintedBlack` (bool)
  - `Finish_Unfinished` (bool fallback)

### D) Output metadata fields (value parity required)

- `MANUFACTURE_COMPONENT` -> `iProperty.Custom.ManufactureComponent`
- `CONTROL_MARK` -> `iProperty.Custom.ControlMark`
- `IDENTITY_DESCRIPTION_SHORT` -> `iProperty.Project.Part Number` (and/or custom mirror)
- `IDENTITY_DESCRIPTION` -> `iProperty.Project.Description`
- `SORTING_ORDER` -> `iProperty.Custom.SortingOrder`
- `QTY_MULTIPLIER` -> `iProperty.Custom.QtyMultiplier`
- `COMPONENT_WEIGHT` -> `iProperty.Custom.ComponentWeight`
- `ASSEMBLY_COMPONENT` -> `iProperty.Custom.AssemblyComponent`
- finish outputs -> `iProperty.Custom.Finish` and optional split fields.

## iLogic Output Mapping Contract

The following selector-to-output parity is required:

1. **Routing**
   - If `Route_CIP = True` and `Route_Erection = False`:
     - `ManufactureComponent` contains `CIP`.
     - Description strings reflect cast/CIP issue path.
   - If `Route_Erection = True` and `Route_CIP = False`:
     - `ManufactureComponent` contains `ERECTION`.
     - Description strings reflect erection issue path.
   - If both are false, fallback to legacy/raw-consumable default for plate.
   - If both are true, CIP takes precedence unless project rule overrides.

2. **Finish**
   - `Finish_Galvanized = True` appends/sets galvanized finish output tokens.
   - `Finish_PaintedBlack = True` appends/sets painted black finish output tokens.
   - No finish selectors true -> unfinished/default finish token.
   - Dual finish selection resolves by project priority (galvanized first, then black), with warning flag.

3. **Description generation**
   - `IDENTITY_DESCRIPTION_SHORT` and `IDENTITY_DESCRIPTION` are computed in iLogic from:
     - base part descriptor
     - dimensional summary
     - routing token
     - finish token

## Execution Order (Inventor iLogic)

1. Normalize and validate selectors.
2. Apply unit-normalized geometry values.
3. Apply hole/slot enable and positional values.
4. Resolve routing token and manufacture component value.
5. Resolve finish token(s).
6. Generate description output fields.
7. Push iProperties and mark document dirty.

## Validation Matrix (minimum)

1. Baseline: no holes/slots, default routing, default finish.
2. Hole-only: 1-hole and 4-hole variants with edge/side offsets.
3. Slot-only: vertical and horizontal slot variants.
4. Mixed feature case: holes + slots together.
5. Routing parity:
   - CIP-only
   - Erection-only
   - both false (fallback)
   - both true (precedence rule)
6. Finish parity:
   - galvanized
   - painted black
   - none
   - both selected (priority rule).
7. Metadata parity:
   - verify `MANUFACTURE_COMPONENT`, short/long descriptions, and marks update from selector states.
