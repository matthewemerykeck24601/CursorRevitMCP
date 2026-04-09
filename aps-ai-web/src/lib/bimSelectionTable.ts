import type { SelectedElementSnapshot } from "@/components/ViewerPanel";
import {
  extractFromForgeObjectName,
  isUsableRevitElementId,
} from "@/lib/forge-revit-object-name";

export type BimTableRow = {
  elementId: string;
  category: string;
  family: string;
  type: string;
  controlMark: string;
  controlNumber: string;
};

type AecElementApiRow = { key: string; value: string; source: string };

/**
 * Canonical Revit / shared-parameter display names (lowercase, spaces).
 * Viewer + AEC maps use the same normalization: trim, lower, collapse spaces.
 *
 * ElementID → Revit Element ID (integer id in Revit). Prefer AEC Data Model when
 * the Model Derivative property set does not include it.
 *
 * Category → Revit element category (e.g. Structural Framing, Specialty Equipment).
 *
 * Family → Family Name parameter.
 *
 * Type → Type / Type Name (family type).
 *
 * CONTROL_MARK / CONTROL_NUMBER → shared parameters (exact names as in Revit).
 */
export const REVIT_ELEMENT_ID_KEYS = [
  "element id",
  "elementid",
  "revit element id",
  "element id revit",
  "host element id",
  "ifc guid",
  "ifcguid",
] as const;

export const REVIT_CATEGORY_KEYS = [
  "category",
] as const;

export const REVIT_FAMILY_NAME_KEYS = [
  "family name",
  "family",
  "loadable family name",
  "structural family name",
  "symbol family name",
] as const;

export const REVIT_TYPE_KEYS = [
  "type name",
  "type",
] as const;

export const SHARED_CONTROL_MARK_KEYS = [
  "control_mark",
  "control mark",
  "controlmark",
  "metromont_control_mark",
] as const;

export const SHARED_CONTROL_NUMBER_KEYS = [
  "control_number",
  "control number",
  "controlnumber",
  "metromont_control_number",
] as const;

function normalizeKey(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

function propMap(el: SelectedElementSnapshot): Map<string, string> {
  const m = new Map<string, string>();
  for (const p of el.properties) {
    const k = normalizeKey(p.displayName);
    if (!m.has(k)) m.set(k, p.displayValue);
  }
  return m;
}

/**
 * Forge/Revit property panels often use "Family" without "Family Name", etc.
 * Duplicate into canonical keys used by pickParameter.
 */
function enrichViewerPropMap(m: Map<string, string>): void {
  const get = (logical: string) => m.get(normalizeKey(logical))?.trim() ?? "";

  if (!get("family name") && get("family")) {
    m.set("family name", get("family"));
  }
  if (!get("type name") && get("type")) {
    m.set("type name", get("type"));
  }

  if (!get("element id")) {
    for (const alt of [
      "elementid",
      "revit element id",
      "element id revit",
      "host element id",
      "ifc guid",
      "ifcguid",
    ]) {
      const v = get(alt);
      if (v) {
        m.set("element id", v);
        break;
      }
    }
  }

  if (!get("control mark")) {
    for (const alt of ["control_mark", "controlmark", "metromont_control_mark"]) {
      const v = get(alt);
      if (v) {
        m.set("control mark", v);
        break;
      }
    }
  }
  if (!get("control number")) {
    for (const alt of ["control_number", "controlnumber", "metromont_control_number"]) {
      const v = get(alt);
      if (v) {
        m.set("control number", v);
        break;
      }
    }
  }
}

/** Exact display-name match only (parseable, no fuzzy substring collisions). */
function getRevitParameter(
  m: Map<string, string>,
  candidates: readonly string[],
): string {
  for (const c of candidates) {
    const key = normalizeKey(c.replace(/_/g, " "));
    const v = m.get(key);
    if (v != null && String(v).trim() !== "") return String(v);
  }
  return "";
}

const RESULT_PROP = /results\[(\d+)\]\.(name|value)$/;

/**
 * Rebuilds a name→value map from AEC element API rows (flattened GraphQL element
 * with properties.results[n].name / .value pairs).
 */
export function aecDataRowsToPropMap(rows: AecElementApiRow[]): Map<string, string> {
  const m = new Map<string, string>();
  const byIdx = new Map<
    number,
    Partial<{ name: string; value: string }>
  >();

  for (const r of rows) {
    const match = r.key.match(RESULT_PROP);
    if (match) {
      const i = Number(match[1]);
      const field = match[2] as "name" | "value";
      const row = byIdx.get(i) ?? {};
      row[field] = r.value;
      byIdx.set(i, row);
    }
  }

  for (const part of byIdx.values()) {
    if (part.name && part.value != null && String(part.value).trim() !== "") {
      const k = normalizeKey(part.name);
      if (!m.has(k)) m.set(k, String(part.value));
    }
  }

  return m;
}

type SourceOrder = "aecFirst" | "viewerFirst";

function pickParameter(
  viewerM: Map<string, string>,
  aecM: Map<string, string>,
  candidates: readonly string[],
  order: SourceOrder,
): string {
  const primary = order === "aecFirst" ? aecM : viewerM;
  const secondary = order === "aecFirst" ? viewerM : aecM;
  return (
    getRevitParameter(primary, candidates) ||
    getRevitParameter(secondary, candidates)
  );
}

export type BuildBimTableOptions = {
  /** Per dbId: rows from GET .../aec-element (AEC Data Model + GraphQL element). */
  aecRowsByDbId?: Record<number, AecElementApiRow[]>;
};

export function buildBimTableRows(
  elements: SelectedElementSnapshot[],
  options?: BuildBimTableOptions,
): BimTableRow[] {
  return elements.map((el) => {
    const viewerM = propMap(el);
    enrichViewerPropMap(viewerM);
    const forgeMeta = extractFromForgeObjectName(el.name);
    if (!isUsableRevitElementId(viewerM.get("element id"))) {
      if (forgeMeta.elementId) {
        viewerM.set("element id", forgeMeta.elementId);
      }
    }
    const hasFamily =
      (viewerM.get("family name")?.trim() ||
        viewerM.get("family")?.trim() ||
        "") !== "";
    if (!hasFamily && forgeMeta.familyHint) {
      viewerM.set("family name", forgeMeta.familyHint);
    }

    const aecM =
      options?.aecRowsByDbId?.[el.dbId] != null
        ? aecDataRowsToPropMap(options.aecRowsByDbId[el.dbId]!)
        : new Map<string, string>();
    enrichViewerPropMap(aecM);

    const pickedElementId = pickParameter(
      viewerM,
      aecM,
      REVIT_ELEMENT_ID_KEYS,
      "aecFirst",
    );
    let elementId = pickedElementId;
    if (!isUsableRevitElementId(elementId)) {
      elementId = forgeMeta.elementId ?? "";
    }
    if (!isUsableRevitElementId(elementId)) {
      elementId = String(el.dbId);
    }
    if (!isUsableRevitElementId(elementId) && el.externalId?.trim()) {
      elementId = el.externalId.trim();
    }

    return {
      elementId,
      category: pickParameter(
        viewerM,
        aecM,
        REVIT_CATEGORY_KEYS,
        "aecFirst",
      ),
      family: pickParameter(
        viewerM,
        aecM,
        REVIT_FAMILY_NAME_KEYS,
        "aecFirst",
      ),
      type: pickParameter(viewerM, aecM, REVIT_TYPE_KEYS, "aecFirst"),
      controlMark: pickParameter(
        viewerM,
        aecM,
        SHARED_CONTROL_MARK_KEYS,
        "aecFirst",
      ),
      controlNumber: pickParameter(
        viewerM,
        aecM,
        SHARED_CONTROL_NUMBER_KEYS,
        "aecFirst",
      ),
    };
  });
}

function csvEscape(s: string): string {
  if (/[",\n\r]/.test(s)) return `"${s.replace(/"/g, '""')}"`;
  return s;
}

export const BIM_TABLE_HEADERS = [
  "ElementID",
  "Category",
  "Family",
  "Type",
  "CONTROL_MARK",
  "CONTROL_NUMBER",
] as const;

export function bimRowsToCsv(rows: BimTableRow[]): string {
  const lines = [
    BIM_TABLE_HEADERS.join(","),
    ...rows.map((r) =>
      [
        r.elementId,
        r.category,
        r.family,
        r.type,
        r.controlMark,
        r.controlNumber,
      ]
        .map((c) => csvEscape(String(c)))
        .join(","),
    ),
  ];
  return lines.join("\r\n");
}
