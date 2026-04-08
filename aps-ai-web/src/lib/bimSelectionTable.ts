import type { SelectedElementSnapshot } from "@/components/ViewerPanel";

export type BimTableRow = {
  elementId: string;
  category: string;
  family: string;
  type: string;
  controlMark: string;
  controlNumber: string;
};

function propMap(el: SelectedElementSnapshot): Map<string, string> {
  const m = new Map<string, string>();
  for (const p of el.properties) {
    const k = p.displayName.trim().toLowerCase().replace(/\s+/g, " ");
    if (!m.has(k)) m.set(k, p.displayValue);
  }
  return m;
}

function getFromMap(m: Map<string, string>, candidates: string[]): string {
  for (const c of candidates) {
    const key = c.toLowerCase().replace(/_/g, " ");
    const v = m.get(key);
    if (v != null && String(v).trim() !== "") return String(v);
  }
  const normalized = candidates.map((c) => c.toLowerCase().replace(/_/g, " "));
  for (const sub of normalized) {
    for (const [k, v] of m) {
      if (!String(v).trim()) continue;
      if (k === sub || k.includes(sub)) return String(v);
    }
  }
  return "";
}

export function buildBimTableRows(
  elements: SelectedElementSnapshot[],
): BimTableRow[] {
  return elements.map((el) => {
    const m = propMap(el);
    const elementId =
      (el.externalId && el.externalId.trim()) ||
      getFromMap(m, ["element id", "elementid"]) ||
      String(el.dbId);
    return {
      elementId,
      category: getFromMap(m, ["category"]),
      family: getFromMap(m, ["family name", "family"]),
      type: getFromMap(m, ["type name", "type"]),
      controlMark: getFromMap(m, ["control_mark", "control mark"]),
      controlNumber: getFromMap(m, ["control_number", "control number"]),
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
