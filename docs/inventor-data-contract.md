# Revit To Inventor Parameter Data Contract

## Purpose

Define the canonical payload used to move Revit family parameters into Inventor parameter replication tools.

## Canonical Payload

```json
{
  "source": "revit-public-mcp",
  "familyName": "DoubleTee",
  "typeName": "DT-12x48",
  "parameters": [
    {
      "name": "Length",
      "dataType": "Number",
      "unitType": "Length",
      "group": "Dimensions",
      "formula": null,
      "isInstance": false,
      "value": 14630
    }
  ]
}
```

## Mapping Rules (Phase 1)

- `name` -> `InventorParameterRecord.name`
- `formula` (if present) -> `expression`
- fallback `value` -> `expression` as string
- `isInstance=false` -> `isKey=true`
- `unitType` conversion hints:
  - `Length` -> `mm`
  - `Area` -> `mm^2`
  - `Volume` -> `mm^3`
  - `Angle` -> `deg`

## Validation Rules

- parameter names must be non-empty and unique per payload
- unsupported value types are discarded
- null/empty formulas are ignored
- unknown units pass through as empty and require manual review

## Sync Metadata

Excel rows should include:

- `parameterSetId`
- `documentName`
- `assemblyName`
- `updatedAtUtc`
- `name`, `units`, `expression`, `value`, `isKey`, `comment`

## Future SQL/ERP Extension

`IParameterSource` is the stable abstraction for data sources:

- `pull(request)` -> list of `InventorParameterRecord`
- `push(request)` -> updated row count

SQL integration should implement this interface without changing MCP tool contracts.
