import { resolveMetromontProductCode } from "@/lib/metromont-family-product-map";

/**
 * Client-side Metromont rules for filtering Forge Viewer property results.
 * Aligns with precast ontology: primary pieces live under Structural Framing.
 */

export type MetromontPieceKind =
  | "COLUMN"
  | "WPA"
  | "WPB"
  | "CLA"
  | "ANY_STRUCTURAL_FRAMING";

function norm(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

/** Flatten APS viewer getProperties / getBulkProperties rows. */
export function propsMapFromViewerProperties(
  properties?: Array<{ displayName?: string; displayValue?: unknown }>,
): Record<string, string> {
  const out: Record<string, string> = {};
  for (const p of properties ?? []) {
    const n = p.displayName ?? "";
    if (!n) continue;
    out[n] = p.displayValue == null ? "" : String(p.displayValue);
  }
  return out;
}

function categoryOf(m: Record<string, string>): string {
  return norm(m["Category"] ?? m["Category Name"] ?? "");
}

/**
 * True when Revit category is Structural Framing (precast "pieces" host).
 * Excludes generic Structural Columns category used for non-precast columns.
 */
export function isStructuralFramingPieceCategory(m: Record<string, string>): boolean {
  const c = categoryOf(m);
  if (!c.includes("structural framing")) return false;
  return true;
}

/**
 * Optional stricter check: drop Revit "Structural Columns" when only that label matches.
 */
export function isExcludedNonPrecastColumnCategory(m: Record<string, string>): boolean {
  const c = categoryOf(m);
  return (
    c.includes("structural column") &&
    !c.includes("framing")
  );
}

function familyTypeProductMark(m: Record<string, string>): string {
  const family = norm(m["Family"] ?? m["Family Name"] ?? "");
  const type = norm(m["Type Name"] ?? m["Type"] ?? "");
  const product = norm(m["CONSTRUCTION_PRODUCT"] ?? "");
  const mark = norm(m["CONTROL_MARK"] ?? "");
  return `${family} ${type} ${product} ${mark}`;
}

function isWallPanelFamily(family: string): boolean {
  return (
    family.includes("wall_panel") ||
    family.includes("wall panel")
  );
}

function isInsulatedWallPanel(
  family: string,
  type: string,
  product: string,
): boolean {
  const hay = `${family} ${type} ${product}`;
  return hay.includes("insulated");
}

/** After Structural Framing is confirmed, match product/column intent. */
export function matchesMetromontPieceKind(
  m: Record<string, string>,
  kind: MetromontPieceKind,
): boolean {
  if (isExcludedNonPrecastColumnCategory(m)) return false;
  if (!isStructuralFramingPieceCategory(m)) return false;

  const family = norm(m["Family"] ?? m["Family Name"] ?? "");
  const type = norm(m["Type Name"] ?? m["Type"] ?? "");
  const product = norm(m["CONSTRUCTION_PRODUCT"] ?? "");
  const mark = norm(m["CONTROL_MARK"] ?? "");
  const hay = familyTypeProductMark(m);
  const productCode = resolveMetromontProductCode(family, product);
  const wallPanelFamily = isWallPanelFamily(family);
  const insulatedPanel = isInsulatedWallPanel(family, type, product);

  switch (kind) {
    case "ANY_STRUCTURAL_FRAMING":
      return true;
    case "COLUMN":
      return family.includes("column") || hay.includes("cla");
    case "WPA":
      // Solid wall panels: prefer explicit WPA/product hints, otherwise
      // wall-panel families that are not insulated.
      return (
        productCode === "WPA" ||
        mark.startsWith("wpa") ||
        (wallPanelFamily && !insulatedPanel)
      );
    case "WPB":
      // Insulated wall panels: explicit WPB/product hints or insulated family/type.
      return (
        productCode === "WPB" ||
        mark.startsWith("wpb") ||
        insulatedPanel
      );
    case "CLA":
      return hay.includes("cla");
    default:
      return false;
  }
}
