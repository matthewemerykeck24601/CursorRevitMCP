/**
 * Revit → Model Derivative → Forge Viewer often encodes the Revit element id in the
 * object display name, e.g. "Family : Type [123456]" or "Basic Wall [123456]".
 * @see https://thebuildingcoder.typepad.com/blog/2019/07/element-identifiers-in-rvt-ifc-nw-and-forge.html
 */

export function extractFromForgeObjectName(name?: string | null): {
  elementId?: string;
  familyHint?: string;
} {
  if (!name?.trim()) return {};
  const trimmed = name.trim();

  const bracketed = trimmed.match(/^(.*)\[(\d+)\]\s*$/);
  if (bracketed) {
    const left = bracketed[1].trim();
    const num = bracketed[2];
    if (num === "0") {
      return {};
    }
    const colon = left.indexOf(":");
    const familyHint =
      colon >= 0
        ? left.slice(0, colon).trim()
        : left.length > 0
          ? left
          : undefined;
    return {
      elementId: num,
      familyHint: familyHint || undefined,
    };
  }

  const colon = trimmed.indexOf(":");
  if (colon > 0) {
    return { familyHint: trimmed.slice(0, colon).trim() };
  }

  return {};
}

export function isUsableRevitElementId(value: string | undefined | null): boolean {
  const t = String(value ?? "").trim();
  if (!t) return false;
  if (t === "0") return false;
  return true;
}

/**
 * Names for {@link https://aps.autodesk.com/blog/getbulkproperties-method model.getBulkProperties}
 * (exact attribute names as stored in the derivative property database).
 */
export const FORGE_VIEWER_BULK_PROPERTY_NAMES = [
  "Element Id",
  "Element ID",
  "Family Name",
  "Family",
  "Type Name",
  "Type",
  "Category",
  "CONTROL_MARK",
  "CONTROL_NUMBER",
] as const;

function normKey(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

/** Merge {@link https://aps.autodesk.com/blog/getbulkproperties-method getBulkProperties} rows into a getProperties list (dedupe by display name). */
export function mergeForgeBulkProperties(
  base: Array<{ displayName: string; displayValue: string; units?: string }>,
  bulk: Array<{
    displayName?: string;
    displayValue?: unknown;
    attributeName?: string;
    units?: string;
  }>,
): Array<{ displayName: string; displayValue: string; units?: string }> {
  const keys = new Set(base.map((p) => normKey(p.displayName)));
  const out = [...base];
  for (const p of bulk) {
    const dn = String(p.displayName ?? p.attributeName ?? "").trim();
    if (!dn) continue;
    const k = normKey(dn);
    if (keys.has(k)) continue;
    keys.add(k);
    out.push({
      displayName: dn,
      displayValue: String(p.displayValue ?? ""),
      units: p.units,
    });
  }
  return out;
}
