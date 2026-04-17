export type MetromontProductCode =
  | "ALA"
  | "AUA"
  | "CLA"
  | "DTA"
  | "FCA"
  | "FSA"
  | "FTA"
  | "LGA"
  | "MDK"
  | "MWV"
  | "RBA"
  | "RTP"
  | "SPA"
  | "STA"
  | "STX"
  | "SWA"
  | "SWF"
  | "TGA"
  | "WPA"
  | "WPB";

type FamilyRule = {
  familyEquals?: string;
  familyStartsWith?: string;
  constructionProductIncludes?: string[];
  productCode: MetromontProductCode;
  constructionProductHint: string;
  // Type names are intentionally excluded from deterministic logic.
  typeNameMode: "variable";
};

/**
 * Normalized import from ProductTable.csv.
 * - Family is authoritative.
 * - Type is treated as variable.
 * - Construction product only disambiguates true family-code collisions.
 */
const FAMILY_RULES: FamilyRule[] = [
  { familyEquals: "COLUMN", productCode: "CLA", constructionProductHint: "COLUMN", typeNameMode: "variable" },
  {
    familyEquals: "DOUBLE_TEE_CHAMFERED_NOMINAL",
    constructionProductIncludes: ["FIELD TOPPED"],
    productCode: "DTA",
    constructionProductHint: "DOUBLE TEE - FIELD TOPPED",
    typeNameMode: "variable",
  },
  { familyEquals: "DOUBLE_TEE_CHAMFERED_NOMINAL", productCode: "FTA", constructionProductHint: "DOUBLE TEE", typeNameMode: "variable" },
  { familyEquals: "FLAT_SLAB", productCode: "FSA", constructionProductHint: "FLAT SLAB", typeNameMode: "variable" },
  { familyEquals: "FLAT_SLAB_WARPING", productCode: "FSA", constructionProductHint: "FLAT SLAB", typeNameMode: "variable" },
  { familyEquals: "H_FRAME", productCode: "SWF", constructionProductHint: "FRAME", typeNameMode: "variable" },
  { familyEquals: "LGIRDER", productCode: "LGA", constructionProductHint: "LGIRDER", typeNameMode: "variable" },
  { familyEquals: "METRODECK", productCode: "MDK", constructionProductHint: "METRODECK", typeNameMode: "variable" },
  { familyEquals: "METROWALL", productCode: "MWV", constructionProductHint: "METROWALL", typeNameMode: "variable" },
  { familyEquals: "RBEAM", productCode: "RBA", constructionProductHint: "RBEAM", typeNameMode: "variable" },
  { familyEquals: "RTP_RADIUSED_NOMINAL", productCode: "RTP", constructionProductHint: "ROOFTOP CHILLER PLATFORM", typeNameMode: "variable" },
  { familyEquals: "SHEARWALL", productCode: "SWA", constructionProductHint: "SHEARWALL", typeNameMode: "variable" },
  { familyEquals: "SPANDREL_BEARING", productCode: "SPA", constructionProductHint: "SPANDREL BEARING", typeNameMode: "variable" },
  { familyEquals: "SPANDREL_BEARING_ADJ_LEDGE", productCode: "SPA", constructionProductHint: "SPANDREL BEARING", typeNameMode: "variable" },
  { familyEquals: "SPANDREL_NONBEARING", productCode: "FCA", constructionProductHint: "SPANDREL NONBEARING", typeNameMode: "variable" },
  { familyEquals: "STAIR_BASE_UNIT", productCode: "STA", constructionProductHint: "STAIR", typeNameMode: "variable" },
  { familyEquals: "STAIR_DROP_IN", productCode: "STA", constructionProductHint: "STAIR", typeNameMode: "variable" },
  { familyEquals: "STAIR_DROP_IN_BASE_UNIT", productCode: "STA", constructionProductHint: "STAIR", typeNameMode: "variable" },
  { familyEquals: "STAIR_Z", productCode: "STX", constructionProductHint: "Z-STAIR", typeNameMode: "variable" },
  { familyEquals: "TGIRDER", productCode: "TGA", constructionProductHint: "TGIRDER", typeNameMode: "variable" },
  { familyEquals: "WALL_CAP", productCode: "WPA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL", productCode: "WPA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL_ANGLED", productCode: "ALA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL_CURVED", productCode: "WPA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyStartsWith: "WALL_PANEL_INSULATED", productCode: "WPB", constructionProductHint: "WALL PANEL INSULATED", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL_LSHAPE_HORIZONTAL", productCode: "ALA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL_LSHAPE_VERTICAL", productCode: "ALA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL_USHAPE_HORIZONTAL", productCode: "AUA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "WALL_PANEL_USHAPE_VERTICAL", productCode: "AUA", constructionProductHint: "WALL PANEL", typeNameMode: "variable" },
  { familyEquals: "Z_SPANDREL_NONBEARING", productCode: "FCA", constructionProductHint: "Z-SPANDREL NONBEARING", typeNameMode: "variable" },
];

function normalizeFamilyToken(value: string): string {
  return value
    .trim()
    .toUpperCase()
    .replace(/\(.*?\)/g, "")
    .replace(/[\s-]+/g, "_")
    .replace(/_+/g, "_")
    .replace(/^_+|_+$/g, "");
}

export function resolveMetromontProductCode(
  familyRaw: string,
  constructionProductRaw?: string,
): MetromontProductCode | null {
  const family = normalizeFamilyToken(familyRaw);
  const constructionProduct = String(constructionProductRaw ?? "").toUpperCase();
  if (!family) return null;
  for (const rule of FAMILY_RULES) {
    if (
      rule.constructionProductIncludes &&
      !rule.constructionProductIncludes.every((token) =>
        constructionProduct.includes(token.toUpperCase()),
      )
    ) {
      continue;
    }
    if (rule.familyEquals && family === rule.familyEquals) return rule.productCode;
    if (
      rule.familyStartsWith &&
      family.startsWith(rule.familyStartsWith)
    ) {
      return rule.productCode;
    }
  }
  return null;
}

