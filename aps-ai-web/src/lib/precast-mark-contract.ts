const PREFIXES = new Set(["WPA", "WPB", "CLA", "ALL"]);

/** Same payloads as aps-ai-web MCP tools; used to enrich Alice chat context. */
export function analyzeProductsAndMarkContract(raw: unknown) {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const p = String(o.product_prefix ?? "ALL");
  const product_prefix = (PREFIXES.has(p) ? p : "ALL") as "WPA" | "WPB" | "CLA" | "ALL";
  const dry_run = o.dry_run !== false;
  return {
    ok: true,
    mode: "schema_contract" as const,
    product_prefix,
    dry_run,
    note:
      "Geometric sameness and CONTROL_MARK assignment require the active Revit document. Use Revit MCP (e.g. send_code_to_revit) with MCPToolHelper mark-verification logic, or APS Design Automation for batch.",
    tolerances: {
      geometry: `±0' - 0.125"`,
      embeddedPosition: `±0' - 0.250"`,
      volumeAddonVolume: 0.5,
      finishArea: 1,
      weight: 25,
      angleDegrees: 1,
    },
    next_steps: dry_run
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

export function getProductSamenessReportContract(raw: unknown) {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const ids = Array.isArray(o.element_ids)
    ? o.element_ids.map((x) => String(x)).filter((s) => s.length > 0)
    : [];
  return {
    ok: true,
    mode: "schema_contract" as const,
    element_ids: ids,
    note:
      "Sameness report must be computed in Revit against Structural Framing instances and nested/intersecting hosts (see SameControlMarkFilterEDrawing, StructuralFramingBoundsObjectEDrawing, ValidStructFraming, ValidVoids).",
    element_count: ids.length,
  };
}

export function assignControlMarksContract(raw: unknown) {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const mark_groups = Array.isArray(o.mark_groups) ? o.mark_groups : [];
  const start =
    typeof o.start_number === "number" && Number.isFinite(o.start_number)
      ? Math.trunc(o.start_number)
      : 100;
  return {
    ok: true,
    mode: "schema_contract" as const,
    mark_groups,
    start_number: start,
    note:
      "Guarded write: apply only after user confirmation; prefer Revit API or APS batch with audit trail. Never assign without verified sameness groups.",
  };
}
