/**
 * AECDM REST row → sameness groups + DA workitem payload.
 * Keep in sync with aps-ai-web/mcp-server/src/lib/aecdmMarkGrouping.ts.
 */
import {
  DA_MARK_PAYLOAD_VERSION,
  MARK_WORKITEM_INTENT,
} from "@/lib/aec-elements-for-marks";

export type AecdmElementRow = {
  dbId: unknown;
  category?: unknown;
  family?: unknown;
  type?: unknown;
  controlMark?: unknown;
  dimLength?: unknown;
  dimHeight?: unknown;
  dimThickness?: unknown;
  externalId?: string | null;
};

export const METROMONT_MARK_TOLERANCE = {
  geometry: 0.125,
  embeddedPosition: 0.25,
  volume: 0.5,
  addonVolume: 0.5,
  finishArea: 1,
  weight: 25,
  angle: 1,
} as const;

function num(v: unknown): number {
  if (typeof v === "number" && !Number.isNaN(v)) return v;
  const n = parseFloat(String(v ?? "").replace(/,/g, ""));
  return Number.isFinite(n) ? n : 0;
}

function normCat(s: unknown): string {
  return String(s ?? "")
    .trim()
    .toLowerCase()
    .replace(/\s+/g, " ");
}

function isStructuralFramingishRow(el: AecdmElementRow): boolean {
  const c = normCat(el.category);
  return (
    c.includes("structural framing") ||
    c.includes("structural column") ||
    c.includes("column")
  );
}

function matchesProductPrefix(
  el: AecdmElementRow,
  prefix: "WPA" | "WPB" | "CLA" | "COLUMN" | "ALL",
): boolean {
  if (prefix === "ALL") return true;
  const hay = `${normCat(el.family)} ${normCat(el.type)}`;
  if (prefix === "COLUMN") {
    return hay.includes("column") || hay.includes("cla");
  }
  return hay.includes(prefix.toLowerCase());
}

export type GroupAecdmParams = {
  elements: AecdmElementRow[];
  product_prefix: "WPA" | "WPB" | "CLA" | "COLUMN" | "ALL";
  modelUrn: string;
  hubId: string;
  dmProjectId: string;
  aecProjectId: string;
};

type MarkEl = {
  aecElementId: string;
  externalId: string;
  name?: string;
};

export function groupAecdmElementsForMarks(params: GroupAecdmParams): {
  proposed_marks: Array<{
    groupId: number;
    control_mark: string;
    count: number;
    elements: MarkEl[];
  }>;
  sameness_groups: Array<{
    sameness_key: string;
    count: number;
    external_ids: string[];
    sample_names: string[];
  }>;
  workitem_arguments: Record<string, unknown>;
  warnings: string[];
  elements_after_filter: number;
  elements_fetched: number;
} {
  const warnings: string[] = [];
  const filtered = params.elements.filter((el) => {
    if (!matchesProductPrefix(el, params.product_prefix)) return false;
    if (params.product_prefix === "ALL" && !isStructuralFramingishRow(el)) {
      return false;
    }
    return true;
  });

  if (filtered.length === 0 && params.elements.length > 0) {
    warnings.push(
      "No AECDM elements matched product_prefix / structural filter after fetch.",
    );
  }

  const elementGroups = new Map<string, AecdmElementRow[]>();
  for (const el of filtered) {
    const L = num(el.dimLength);
    const H = num(el.dimHeight);
    const T = num(el.dimThickness);
    const dimKey = `${Math.round(L * 8) / 8}-${Math.round(H * 8) / 8}-${Math.round(T * 8) / 8}`;
    const groupKey = `${String(el.family ?? "")}-${String(el.type ?? "")}-${dimKey}`;
    const list = elementGroups.get(groupKey) ?? [];
    list.push(el);
    elementGroups.set(groupKey, list);
  }

  let markCounter = 100;
  const prefix =
    params.product_prefix === "ALL"
      ? "MK"
      : params.product_prefix === "COLUMN"
        ? "CLA"
        : params.product_prefix;

  const proposed_marks: Array<{
    groupId: number;
    control_mark: string;
    count: number;
    elements: MarkEl[];
  }> = [];

  const sameness_groups: Array<{
    sameness_key: string;
    count: number;
    external_ids: string[];
    sample_names: string[];
  }> = [];

  const sortedKeys = [...elementGroups.keys()].sort();
  let groupId = 1;
  for (const key of sortedKeys) {
    const members = elementGroups.get(key) ?? [];
    if (members.length === 0) continue;
    const control_mark = `${prefix}${markCounter++}`;
    const markElements: MarkEl[] = members.map((m) => {
      const idStr = m.dbId == null ? "" : String(m.dbId);
      const ext = (m.externalId && String(m.externalId).trim()) || idStr;
      return {
        aecElementId: idStr,
        externalId: ext,
        name: [m.family, m.type].filter(Boolean).join(" / ") || idStr,
      };
    });
    const external_ids = markElements.map((m) => m.externalId).filter(Boolean);

    sameness_groups.push({
      sameness_key: key,
      count: members.length,
      external_ids,
      sample_names: markElements
        .slice(0, 5)
        .map((m) => m.name ?? m.aecElementId),
    });

    proposed_marks.push({
      groupId: groupId++,
      control_mark,
      count: members.length,
      elements: markElements,
    });
  }

  const workitem_arguments: Record<string, unknown> = {
    version: DA_MARK_PAYLOAD_VERSION,
    intent: MARK_WORKITEM_INTENT,
    idResolution: "externalId",
    source: "aecdm_analyze_v1",
    product_prefix: params.product_prefix,
    tolerances: { ...METROMONT_MARK_TOLERANCE },
    provenance: {
      hubId: params.hubId,
      projectId: params.dmProjectId,
      aecProjectId: params.aecProjectId,
      analyzedAt: new Date().toISOString(),
      dataPath: "aecdm_rest",
    },
    marks: proposed_marks.map((g) => ({
      groupId: g.groupId,
      control_mark: g.control_mark,
      externalIds: g.elements.map((e) => e.externalId).filter(Boolean),
      aecElementIds: g.elements.map((e) => e.aecElementId).filter(Boolean),
    })),
    note:
      "AECDM REST + v0 family/type/dimension grouping; enhance with bbox/nested checks in DA (MCPToolHelper).",
  };

  return {
    proposed_marks,
    sameness_groups,
    workitem_arguments,
    warnings,
    elements_after_filter: filtered.length,
    elements_fetched: params.elements.length,
  };
}

/** Fetch design elements via AEC Data Model REST (same path as MCP get_elements_by_category). */
export async function queryAecdmElementsForMarks(params: {
  accessToken: string;
  aecProjectId: string;
  modelUrn: string;
  /** e.g. Structural Framing for Metromont precast pieces */
  category?: string;
  family?: string;
  limit?: number;
}): Promise<AecdmElementRow[]> {
  const designId = params.modelUrn.includes(":")
    ? (params.modelUrn.split(":").pop() ?? params.modelUrn)
    : params.modelUrn;
  const base = `https://developer.api.autodesk.com/aecdm/v1/projects/${encodeURIComponent(params.aecProjectId)}/designs/${encodeURIComponent(designId)}/elements`;
  const url = new URL(base);
  url.searchParams.set("limit", String(params.limit ?? 500));
  if (params.category) {
    url.searchParams.set("filter[category]", params.category);
  }
  if (params.family) {
    url.searchParams.set("filter[family]", params.family);
  }

  const res = await fetch(url.toString(), {
    headers: { Authorization: `Bearer ${params.accessToken}` },
    cache: "no-store",
  });
  if (!res.ok) {
    const t = await res.text();
    throw new Error(`AECDM REST ${res.status}: ${t}`);
  }
  const data = (await res.json()) as { elements?: unknown[] };
  const elements = Array.isArray(data.elements) ? data.elements : [];
  return elements.map((el: unknown) => {
    const row = el as {
      id?: unknown;
      category?: unknown;
      family?: unknown;
      type?: unknown;
      properties?: Record<string, unknown>;
    };
    const props = row.properties ?? {};
    const extRaw = props["External ID"] ?? props.ExternalId ?? props.externalId;
    return {
      dbId: row.id,
      category: row.category,
      family: row.family,
      type: row.type,
      controlMark: props.CONTROL_MARK ?? null,
      externalId:
        extRaw == null || extRaw === ""
          ? null
          : String(extRaw).trim() || null,
      dimLength: props.DIM_LENGTH ?? props.Length ?? props.length,
      dimHeight: props.DIM_HEIGHT ?? props.Height ?? props.height,
      dimThickness:
        props.DIM_THICKNESS ?? props.Thickness ?? props.thickness,
    };
  });
}
