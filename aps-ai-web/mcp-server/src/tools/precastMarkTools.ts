import { z } from "zod";

const AnalyzeArgs = z.object({
  product_prefix: z.enum(["WPA", "WPB", "CLA", "ALL"]),
  dry_run: z.boolean().optional().default(true),
});

const SamenessArgs = z.object({
  element_ids: z.array(z.string()),
});

const AssignArgs = z.object({
  mark_groups: z.array(z.record(z.string(), z.any())),
  start_number: z.number().int().optional().default(100),
});

/** Contract/stub: full mark verification runs in Revit (MCPToolHelper / Revit MCP). */
export function analyzeProductsAndMark(args: unknown) {
  const parsed = AnalyzeArgs.parse(args);
  return {
    ok: true,
    mode: "schema_contract",
    product_prefix: parsed.product_prefix,
    dry_run: parsed.dry_run,
    note:
      "Geometric sameness and CONTROL_MARK assignment require the active Revit document. Use Revit MCP (e.g. send_code_to_revit) with MCPToolHelper mark-verification logic, or APS Design Automation for batch.",
    tolerances: {
      geometry: '±0\' - 0.125"',
      embeddedPosition: '±0\' - 0.250"',
      volumeAddonVolume: 0.5,
      finishArea: 1,
      weight: 25,
      angleDegrees: 1,
    },
    next_steps: parsed.dry_run
      ? [
          "Confirm product_prefix filter and selection scope in Revit.",
          "Run bounds + intersecting-element comparison per precast-revit-ontology.",
        ]
      : [
          "Execute guarded CONTROL_MARK writes only after sameness groups are verified.",
          "Log affected element IDs and families/types in the response.",
        ],
  };
}

export function getProductSamenessReport(args: unknown) {
  const parsed = SamenessArgs.parse(args);
  return {
    ok: true,
    mode: "schema_contract",
    element_ids: parsed.element_ids,
    note:
      "Sameness report must be computed in Revit against Structural Framing instances and nested/intersecting hosts (see SameControlMarkFilterEDrawing, StructuralFramingBoundsObjectEDrawing, ValidStructFraming, ValidVoids).",
    element_count: parsed.element_ids.length,
  };
}

export function assignControlMarks(args: unknown) {
  const parsed = AssignArgs.parse(args);
  return {
    ok: true,
    mode: "schema_contract",
    mark_groups: parsed.mark_groups,
    start_number: parsed.start_number,
    note:
      "Guarded write: apply only after user confirmation; prefer Revit API or APS batch with audit trail. Never assign without verified sameness groups.",
  };
}
