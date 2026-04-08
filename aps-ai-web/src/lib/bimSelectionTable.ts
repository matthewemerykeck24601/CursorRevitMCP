import type { SelectedElementSnapshot } from "@/components/ViewerPanel";

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
] as const;

export const REVIT_CATEGORY_KEYS = [
  "category",
] as const;

export const REVIT_FAMILY_NAME_KEYS = [
  "family name",
  "family",
] as const;

export const REVIT_TYPE_KEYS = [
  "type name",
  "type",
] as const;

export const SHARED_CONTROL_MARK_KEYS = [
  "control_mark",
  "control mark",
] as const;

export const SHARED_CONTROL_NUMBER_KEYS = [
  "control_number",
  "control number",
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
    const aecM =
      options?.aecRowsByDbId?.[el.dbId] != null
        ? aecDataRowsToPropMap(options.aecRowsByDbId[el.dbId]!)
        : new Map<string, string>();

    const elementId = pickParameter(
      viewerM,
      aecM,
      REVIT_ELEMENT_ID_KEYS,
      "aecFirst",
    );

    return {
      elementId,
      category: pickParameter(
        viewerM,
        aecM,
        REVIT_CATEGORY_KEYS,
        "viewerFirst",
      ),
      family: pickParameter(
        viewerM,
        aecM,
        REVIT_FAMILY_NAME_KEYS,
        "viewerFirst",
      ),
      type: pickParameter(viewerM, aecM, REVIT_TYPE_KEYS, "viewerFirst"),
      controlMark: pickParameter(
        viewerM,
        aecM,
        SHARED_CONTROL_MARK_KEYS,
        "viewerFirst",
      ),
      controlNumber: pickParameter(
        viewerM,
        aecM,
        SHARED_CONTROL_NUMBER_KEYS,
        "viewerFirst",
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
