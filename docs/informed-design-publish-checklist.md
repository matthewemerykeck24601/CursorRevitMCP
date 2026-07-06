# Informed Design Publish Checklist (Phase 1 Manual)

## Preconditions

- `inventor_prepare_informed_design_publish` returns:
  - `validation.hasParameters = true`
  - `validation.hasModelStateHint = true`
- active Inventor assembly is saved and up to date
- manufacturer-approved parameters are exposed only

## Publish Steps

1. Open Inventor model and confirm final parameter state.
2. Launch Informed Design add-in in Inventor.
3. Select BIM representation/model state for Revit consumers.
4. Confirm exposed configurable parameters and allowed ranges.
5. Publish to ACC/Fusion Team target folder.
6. Record publish timestamp and run label.

## Post-Publish Verification

1. Load resulting RFA into Revit test project.
2. Validate at least one default and one edge configuration.
3. Confirm type naming and key dimensions match Inventor source.
4. Archive publish metadata with run label and parameter set id.

## Failure Handling

- If publish validation fails, do not publish.
- Fix parameter mapping or model state visibility.
- rerun:
  - `inventor_sync_parameters`
  - `inventor_prepare_informed_design_publish`
