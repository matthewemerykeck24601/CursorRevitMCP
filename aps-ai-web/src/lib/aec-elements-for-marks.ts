import { apsPost, resolveAecProjectId } from "@/lib/aps";

/** Versioned payload for Design Automation workitem `arguments`. */
export const DA_MARK_PAYLOAD_VERSION = "aec_analyze_v1";

export const MARK_WORKITEM_INTENT = "apply_control_marks" as const;

/** Property names requested from AEC GraphQL (aligned with chat MCP reads). */
export const MARK_PROPERTY_NAMES = [
  "External ID",
  "Category",
  "Family Name",
  "Type Name",
  "CONTROL_MARK",
  "CONTROL_NUMBER",
  "Length",
  "Width",
  "Height",
  "CONSTRUCTION_PRODUCT",
  "DESIGN_NUMBER",
  "IDENTITY_DESCRIPTION",
] as const;

const ELEMENTS_PAGE_QUERY = `
  query ElementsForMarks($projectId: ID!, $limit: Int!, $cursor: String) {
    elementsByProject(projectId: $projectId, pagination: { limit: $limit, cursor: $cursor }) {
      pagination { cursor }
      results {
        id
        name
        properties(
          filter: { names: ${JSON.stringify([...MARK_PROPERTY_NAMES])} }
        ) {
          results {
            name
            value
            definition {
              units { name }
            }
          }
        }
      }
    }
  }
`;

export type ProductPrefix = "WPA" | "WPB" | "CLA" | "ALL";

export type AecElementForMark = {
  aecElementId: string;
  name?: string;
  props: Record<string, string>;
};

export type SamenessGroupV0 = {
  sameness_key: string;
  count: number;
  external_ids: string[];
  sample_names: string[];
};

export type ProposedMarkGroup = {
  groupId: number;
  control_mark: string;
  count: number;
  elements: Array<{
    aecElementId: string;
    externalId: string;
    name?: string;
  }>;
};

export type AecMarkAnalysisResult = {
  aec_project_id: string;
  elements_fetched: number;
  elements_after_filter: number;
  sameness_groups: SamenessGroupV0[];
  proposed_marks: ProposedMarkGroup[];
  warnings: string[];
  /** Canonical JSON for DA `arguments` (stringify when posting workitem). */
  workitem_arguments: Record<string, unknown>;
};

type GqlElement = {
  id?: string;
  name?: string;
  properties?: {
    results?: Array<{ name?: string; value?: unknown }>;
  };
};

type GqlPage = {
  data?: {
    elementsByProject?: {
      pagination?: { cursor?: string | null };
      results?: GqlElement[];
    };
  };
  errors?: Array<{ message?: string }>;
};

function propsToMap(el: GqlElement): Record<string, string> {
  const out: Record<string, string> = {};
  for (const row of el.properties?.results ?? []) {
    const n = row.name ?? "";
    if (!n) continue;
    out[n] = row.value == null ? "" : String(row.value);
  }
  return out;
}

function norm(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

function matchesProductPrefix(props: Record<string, string>, prefix: ProductPrefix): boolean {
  if (prefix === "ALL") return true;
  const family = norm(props["Family Name"] ?? "");
  const type = norm(props["Type Name"] ?? "");
  const product = norm(props["CONSTRUCTION_PRODUCT"] ?? "");
  const hay = `${family} ${type} ${product}`;
  const p = prefix.toLowerCase();
  return hay.includes(p.toLowerCase());
}

function isStructuralFramingish(props: Record<string, string>): boolean {
  const cat = norm(props["Category"] ?? "");
  return cat.includes("structural framing") || cat.includes("structural column") || cat.includes("column");
}

/** v0 sameness: type + normalized dimensions + design number (advisory until Revit geometry). */
function samenessKeyV0(props: Record<string, string>): string {
  const parts = [
    norm(props["Type Name"] ?? ""),
    norm(props["Family Name"] ?? ""),
    norm(props["Length"] ?? ""),
    norm(props["Width"] ?? ""),
    norm(props["Height"] ?? ""),
    norm(props["DESIGN_NUMBER"] ?? ""),
  ];
  return parts.join("|");
}

function markPrefixForGroup(prefix: ProductPrefix, props: Record<string, string>): string {
  if (prefix !== "ALL") return prefix;
  const fam = props["Family Name"] ?? "";
  if (/^WPA/i.test(fam)) return "WPA";
  if (/^WPB/i.test(fam)) return "WPB";
  if (/^CLA/i.test(fam)) return "CLA";
  return "WPA";
}

/**
 * Paginated AEC GraphQL fetch + product filter + v0 property-based sameness + CONTROL_MARK proposals (start at 100).
 */
export async function analyzeAecElementsForMarks(params: {
  accessToken: string;
  hubId: string;
  projectId: string;
  product_prefix: ProductPrefix;
  pageSize?: number;
  maxPages?: number;
}): Promise<AecMarkAnalysisResult> {
  const pageSize = Math.min(Math.max(params.pageSize ?? 100, 1), 500);
  const maxPages = Math.min(Math.max(params.maxPages ?? 50, 1), 200);
  const warnings: string[] = [];

  const aecProjectId = await resolveAecProjectId(
    params.accessToken,
    params.hubId,
    params.projectId,
  );

  const all: AecElementForMark[] = [];
  let cursor: string | null | undefined = undefined;

  for (let page = 0; page < maxPages; page++) {
    const variables: {
      projectId: string;
      limit: number;
      cursor: string | null;
    } = {
      projectId: aecProjectId,
      limit: pageSize,
      cursor: cursor ?? null,
    };

    const gql = await apsPost<GqlPage>("/aec/graphql", params.accessToken, {
      query: ELEMENTS_PAGE_QUERY,
      variables,
    });

    if (gql.errors?.length) {
      warnings.push(
        gql.errors.map((e) => e.message ?? "GraphQL error").join("; "),
      );
      break;
    }

    const block = gql.data?.elementsByProject;
    const results = block?.results ?? [];
    for (const r of results) {
      const id = r.id;
      if (!id) continue;
      const props = propsToMap(r);
      all.push({
        aecElementId: id,
        name: r.name,
        props,
      });
    }

    const nextCursor = block?.pagination?.cursor;
    if (!nextCursor || results.length === 0) break;
    cursor = nextCursor;
  }

  const filtered = all.filter((e) => {
    if (!matchesProductPrefix(e.props, params.product_prefix)) return false;
    if (params.product_prefix === "ALL" && !isStructuralFramingish(e.props)) {
      return false;
    }
    return true;
  });

  if (filtered.length === 0 && all.length > 0) {
    warnings.push(
      "No elements matched product_prefix after filter; try ALL or verify Family/Type naming.",
    );
  }

  const groupMap = new Map<string, AecElementForMark[]>();
  for (const el of filtered) {
    const key = samenessKeyV0(el.props);
    const list = groupMap.get(key) ?? [];
    list.push(el);
    groupMap.set(key, list);
  }

  const sameness_groups: SamenessGroupV0[] = [];
  const proposed_marks: ProposedMarkGroup[] = [];
  let groupId = 1;
  let markNum = 100;

  const sortedKeys = [...groupMap.keys()].sort();
  for (const key of sortedKeys) {
    const members = groupMap.get(key) ?? [];
    if (members.length === 0) continue;
    const sample = members.slice(0, 5).map((m) => m.name ?? m.aecElementId);
    const external_ids = members
      .map((m) => m.props["External ID"]?.trim())
      .filter((x): x is string => Boolean(x));

    sameness_groups.push({
      sameness_key: key,
      count: members.length,
      external_ids,
      sample_names: sample,
    });

    const mp = markPrefixForGroup(params.product_prefix, members[0].props);
    const control_mark = `${mp}${markNum}`;
    proposed_marks.push({
      groupId: groupId++,
      control_mark,
      count: members.length,
      elements: members.map((m) => ({
        aecElementId: m.aecElementId,
        externalId: m.props["External ID"]?.trim() ?? "",
        name: m.name,
      })),
    });
    markNum += 1;
  }

  const workitem_arguments: Record<string, unknown> = {
    version: DA_MARK_PAYLOAD_VERSION,
    intent: MARK_WORKITEM_INTENT,
    idResolution: "externalId",
    source: "aec_analyze_v1",
    product_prefix: params.product_prefix,
    provenance: {
      hubId: params.hubId,
      projectId: params.projectId,
      aecProjectId,
      analyzedAt: new Date().toISOString(),
    },
    marks: proposed_marks.map((g) => ({
      groupId: g.groupId,
      control_mark: g.control_mark,
      externalIds: g.elements.map((e) => e.externalId).filter(Boolean),
      aecElementIds: g.elements.map((e) => e.aecElementId),
    })),
    note: "v0 property-based sameness; validate in Revit DA with MCPToolHelper tolerances.",
  };

  return {
    aec_project_id: aecProjectId,
    elements_fetched: all.length,
    elements_after_filter: filtered.length,
    sameness_groups,
    proposed_marks,
    warnings,
    workitem_arguments,
  };
}
