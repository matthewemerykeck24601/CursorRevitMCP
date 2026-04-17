import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsPost, queryAecDataModel, resolveAecProjectId } from "@/lib/aps";
import {
  extractFromForgeObjectName,
  isUsableRevitElementId,
} from "@/lib/forge-revit-object-name";

export const runtime = "nodejs";

type DataRow = {
  key: string;
  value: string;
  source: string;
};

function flatten(input: unknown, source: string, limit = 200): DataRow[] {
  const out: DataRow[] = [];
  const walk = (v: unknown, path: string) => {
    if (out.length >= limit) return;
    if (v === null || v === undefined) {
      out.push({ key: path, value: String(v), source });
      return;
    }
    if (typeof v !== "object") {
      out.push({ key: path, value: String(v), source });
      return;
    }
    if (Array.isArray(v)) {
      v.forEach((item, i) => walk(item, `${path}[${i}]`));
      return;
    }
    for (const [k, child] of Object.entries(v as Record<string, unknown>)) {
      walk(child, path ? `${path}.${k}` : k);
      if (out.length >= limit) break;
    }
  };
  walk(input, "");
  return out;
}

function findMatchingObjects(
  input: unknown,
  needleExternalId: string,
  needleDbId: string,
  maxMatches = 3,
): unknown[] {
  const matches: unknown[] = [];
  const nExternal = needleExternalId.toLowerCase();
  const nDb = needleDbId.toLowerCase();

  const visit = (v: unknown) => {
    if (matches.length >= maxMatches) return;
    if (!v || typeof v !== "object") return;

    const json = JSON.stringify(v).toLowerCase();
    const hitExternal = nExternal.length > 0 && json.includes(nExternal);
    const hitDb = nDb.length > 0 && json.includes(nDb);
    if (hitExternal || hitDb) {
      matches.push(v);
      return;
    }

    if (Array.isArray(v)) {
      for (const child of v) visit(child);
      return;
    }
    for (const child of Object.values(v as Record<string, unknown>)) {
      visit(child);
      if (matches.length >= maxMatches) break;
    }
  };
  visit(input);
  return matches;
}

type GraphElement = {
  id?: string;
  name?: string;
  properties?: {
    results?: Array<{
      name?: string;
      value?: unknown;
      definition?: { units?: { name?: string } };
    }>;
  };
};

type GraphProperty = {
  name?: string;
  value?: unknown;
  definition?: { units?: { name?: string } };
};

type AecElementScanResponse = {
  data?: {
    elementsByProject?: {
      pagination?: { cursor?: string | null };
      results?: GraphElement[];
    };
  };
};

type ElementGroupResult = {
  id?: string;
  alternativeIdentifiers?: {
    fileVersionUrn?: string;
    fileUrn?: string;
  };
};

type ElementGroupsByProjectResponse = {
  data?: {
    elementGroupsByProject?: {
      results?: ElementGroupResult[];
    };
  };
};

const ELEMENT_SCAN_PROPERTY_NAMES = [
  "External ID",
  "Element Id",
  "Element ID",
  "CONTROL_MARK",
  "CONTROL_NUMBER",
  "CONTROL MARK",
  "Control Mark",
  "Category",
  "Family Name",
  "Family",
  "Type Name",
  "Type",
  "Length",
  "Width",
  "Height",
] as const;

function normElementName(s: string): string {
  return s.trim().toLowerCase().replace(/\s+/g, " ");
}

function normalizeKey(s: string): string {
  return s.trim().toLowerCase().replace(/[\s_-]+/g, "");
}

function getPropertyValue(
  props: GraphProperty[],
  candidateNames: string[],
): string {
  const byName = new Map<string, GraphProperty>();
  for (const prop of props) {
    const key = normalizeKey(String(prop.name ?? ""));
    if (!key || byName.has(key)) continue;
    byName.set(key, prop);
  }
  for (const candidate of candidateNames) {
    const match = byName.get(normalizeKey(candidate));
    if (!match) continue;
    const value = String(match.value ?? "").trim();
    if (!value) continue;
    return value;
  }
  return "";
}

function buildElementSchemaRows(params: {
  element: GraphElement;
  requestDbId: string;
  requestElementName: string;
}): DataRow[] {
  const props = (params.element.properties?.results ?? []) as GraphProperty[];
  const internalFromProps = getPropertyValue(props, [
    "Element Id",
    "Element ID",
    "Revit Element Id",
  ]);
  const parsedFromName = extractFromForgeObjectName(
    params.requestElementName || params.element.name || "",
  ).elementId;
  const internalElementId = isUsableRevitElementId(internalFromProps)
    ? internalFromProps
    : isUsableRevitElementId(parsedFromName)
      ? String(parsedFromName)
      : isUsableRevitElementId(params.requestDbId)
        ? params.requestDbId
        : "(unknown)";
  const elementCategory =
    getPropertyValue(props, ["Category", "Category Name"]) || "(unknown)";
  const elementName =
    getPropertyValue(props, ["Family Name", "Family"]) ||
    params.requestElementName ||
    params.element.name ||
    "(unknown)";
  const elementType =
    getPropertyValue(props, ["Type Name", "Type"]) || "(unknown)";

  const rows: DataRow[] = [
    { key: "internalElementID", value: internalElementId, source: "aecdatamodel" },
    { key: "elementCategory", value: elementCategory, source: "aecdatamodel" },
    { key: "elementName", value: elementName, source: "aecdatamodel" },
    { key: "elementType", value: elementType, source: "aecdatamodel" },
  ];

  for (const prop of props) {
    const name = String(prop.name ?? "").trim();
    if (!name) continue;
    const value = String(prop.value ?? "").trim();
    const units = String(prop.definition?.units?.name ?? "").trim();
    rows.push({
      key: `parameter.${name}`,
      value: units && value ? `${value} ${units}` : value || "(empty)",
      source: "aecdatamodel",
    });
  }

  return rows;
}

/**
 * Resolve AEC GraphQL element: prefer External ID match, then viewer element name (Forge name vs AEC name).
 */
async function fetchElementPaged(
  accessToken: string,
  aecProjectId: string,
  externalId: string,
  elementName: string,
): Promise<{
  element: GraphElement | null;
  pagesScanned: number;
  matchedBy?: "externalId" | "elementName";
}> {
  const query = `
    query ElementScan($projectId: ID!, $cursor: String) {
      elementsByProject(
        projectId: $projectId
        pagination: { limit: 200, cursor: $cursor }
      ) {
        pagination { cursor }
        results {
          id
          name
          properties(filter: { names: ${JSON.stringify([...ELEMENT_SCAN_PROPERTY_NAMES])} }) {
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

  const targetExt = externalId.trim().toLowerCase();
  const targetName = normElementName(elementName);
  let pagesScanned = 0;

  const runScan = async (
    mode: "external" | "name",
  ): Promise<GraphElement | null> => {
    let cursor: string | null = null;
    for (let p = 0; p < 60; p += 1) {
      pagesScanned += 1;
      const scanResponse: AecElementScanResponse = (await apsPost(
        "/aec/graphql",
        accessToken,
        {
          query,
          variables: { projectId: aecProjectId, cursor },
        },
      )) as AecElementScanResponse;
      const page = scanResponse.data?.elementsByProject?.results ?? [];
      for (const element of page) {
        if (mode === "external") {
          if (!targetExt) continue;
          const props = element.properties?.results ?? [];
          const extValue = props.find(
            (prop) => String(prop.name ?? "").toLowerCase() === "external id",
          )?.value;
          if (String(extValue ?? "").trim().toLowerCase() === targetExt) {
            return element;
          }
        } else {
          if (targetName.length < 4) continue;
          const en = normElementName(element.name ?? "");
          if (en === targetName) {
            return element;
          }
          if (targetName.length >= 12 || en.length >= 12) {
            if (en.includes(targetName) || targetName.includes(en)) {
              return element;
            }
          }
        }
      }
      const next =
        scanResponse.data?.elementsByProject?.pagination?.cursor ?? null;
      if (!next || next === cursor) break;
      cursor = next;
    }
    return null;
  };

  if (targetExt) {
    const hit = await runScan("external");
    if (hit) {
      return { element: hit, pagesScanned, matchedBy: "externalId" };
    }
  }

  if (targetName.length >= 4) {
    const hit = await runScan("name");
    if (hit) {
      return { element: hit, pagesScanned, matchedBy: "elementName" };
    }
  }

  return { element: null, pagesScanned };
}

function normalizeUrn(value: string): string {
  return decodeURIComponent(value ?? "").trim().toLowerCase();
}

async function resolveElementGroupIdForVersion(params: {
  accessToken: string;
  projectId: string;
  versionId: string;
}): Promise<string | null> {
  const query = `
    query ElementGroupsByProjectForVersion($projectId: ID!) {
      elementGroupsByProject(projectId: $projectId) {
        results {
          id
          alternativeIdentifiers {
            fileVersionUrn
            fileUrn
          }
        }
      }
    }
  `;
  const response = (await apsPost("/aec/graphql", params.accessToken, {
    query,
    variables: { projectId: params.projectId },
  })) as ElementGroupsByProjectResponse;
  const target = normalizeUrn(params.versionId);
  const groups = response.data?.elementGroupsByProject?.results ?? [];
  const match = groups.find((group) => {
    const fileVersionUrn = normalizeUrn(group.alternativeIdentifiers?.fileVersionUrn ?? "");
    return fileVersionUrn === target || fileVersionUrn.includes(target) || target.includes(fileVersionUrn);
  });
  return match?.id ? String(match.id) : null;
}

async function fetchElementByElementGroupPaged(params: {
  accessToken: string;
  elementGroupId: string;
  externalId: string;
  elementName: string;
}): Promise<{
  element: GraphElement | null;
  pagesScanned: number;
  matchedBy?: "externalId" | "elementName";
}> {
  const query = `
    query ElementScanByElementGroup($elementGroupId: ID!, $cursor: String) {
      elementsByElementGroup(
        elementGroupId: $elementGroupId
        pagination: { limit: 200, cursor: $cursor }
      ) {
        pagination { cursor }
        results {
          id
          name
          properties(filter: { names: ${JSON.stringify([...ELEMENT_SCAN_PROPERTY_NAMES])} }) {
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

  const targetExt = params.externalId.trim().toLowerCase();
  const targetName = normElementName(params.elementName);
  let pagesScanned = 0;
  let cursor: string | null = null;
  for (let p = 0; p < 80; p += 1) {
    pagesScanned += 1;
    const response = (await apsPost("/aec/graphql", params.accessToken, {
      query,
      variables: { elementGroupId: params.elementGroupId, cursor },
    })) as {
      data?: {
        elementsByElementGroup?: {
          pagination?: { cursor?: string | null };
          results?: GraphElement[];
        };
      };
    };
    const page = response.data?.elementsByElementGroup?.results ?? [];
    for (const element of page) {
      if (targetExt) {
        const props = element.properties?.results ?? [];
        const extValue = props.find(
          (prop) => String(prop.name ?? "").toLowerCase() === "external id",
        )?.value;
        if (String(extValue ?? "").trim().toLowerCase() === targetExt) {
          return { element, pagesScanned, matchedBy: "externalId" };
        }
      }
      if (targetName.length >= 4) {
        const en = normElementName(element.name ?? "");
        if (en === targetName || en.includes(targetName) || targetName.includes(en)) {
          return { element, pagesScanned, matchedBy: "elementName" };
        }
      }
    }
    const next = response.data?.elementsByElementGroup?.pagination?.cursor ?? null;
    if (!next || next === cursor) break;
    cursor = next;
  }
  return { element: null, pagesScanned };
}

async function fetchElementWithAllProperties(
  accessToken: string,
  aecProjectId: string,
  elementId: string,
): Promise<{
  element: GraphElement | null;
  pagesScanned: number;
}> {
  const query = `
    query ElementFullPropsScan($projectId: ID!, $cursor: String) {
      elementsByProject(
        projectId: $projectId
        pagination: { limit: 200, cursor: $cursor }
      ) {
        pagination { cursor }
        results {
          id
          name
          properties {
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

  let pagesScanned = 0;
  let cursor: string | null = null;
  for (let p = 0; p < 60; p += 1) {
    pagesScanned += 1;
    const scanResponse: AecElementScanResponse = (await apsPost(
      "/aec/graphql",
      accessToken,
      {
        query,
        variables: { projectId: aecProjectId, cursor },
      },
    )) as AecElementScanResponse;
    const page = scanResponse.data?.elementsByProject?.results ?? [];
    const hit = page.find((element) => String(element.id ?? "") === elementId);
    if (hit) {
      return { element: hit, pagesScanned };
    }
    const next = scanResponse.data?.elementsByProject?.pagination?.cursor ?? null;
    if (!next || next === cursor) break;
    cursor = next;
  }
  return { element: null, pagesScanned };
}

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { projectId } = await context.params;
  const hubId = request.nextUrl.searchParams.get("hubId");
  const itemId = request.nextUrl.searchParams.get("itemId") ?? undefined;
  const versionId = request.nextUrl.searchParams.get("versionId") ?? "";
  const externalId = request.nextUrl.searchParams.get("externalId") ?? "";
  const dbId = request.nextUrl.searchParams.get("dbId") ?? "";
  const elementName = request.nextUrl.searchParams.get("elementName") ?? "";

  if (!hubId) {
    return NextResponse.json(
      { error: "Missing required query parameter: hubId" },
      { status: 400 },
    );
  }

  const aec = await queryAecDataModel(
    auth.session.accessToken,
    hubId,
    projectId,
    itemId,
  );
  const aecProjectId = await resolveAecProjectId(
    auth.session.accessToken,
    hubId,
    projectId,
  );

  const rows: DataRow[] = [];
  let graphScan = { element: null as GraphElement | null, pagesScanned: 0, matchedBy: undefined as
    | "externalId"
    | "elementName"
    | undefined };
  let strategy = "project-scan";
  if (versionId) {
    const elementGroupId = await resolveElementGroupIdForVersion({
      accessToken: auth.session.accessToken,
      projectId: aecProjectId,
      versionId,
    });
    rows.push({
      key: "diagnostics.elementGroupId",
      value: elementGroupId ?? "(not found)",
      source: "aecdatamodel",
    });
    if (elementGroupId) {
      const byGroup = await fetchElementByElementGroupPaged({
        accessToken: auth.session.accessToken,
        elementGroupId,
        externalId,
        elementName,
      });
      graphScan = byGroup;
      strategy = "element-group-scan";
    }
  }
  if (!graphScan.element) {
    graphScan = await fetchElementPaged(
      auth.session.accessToken,
      aecProjectId,
      externalId,
      elementName,
    );
    strategy = "project-scan";
  }
  rows.push({
    key: "diagnostics.externalId",
    value: externalId || "(empty)",
    source: "request",
  });
  rows.push({
    key: "diagnostics.elementName",
    value: elementName || "(empty)",
    source: "request",
  });
  rows.push({
    key: "diagnostics.graphqlMatch",
    value: graphScan.matchedBy ?? "(none)",
    source: "aecdatamodel",
  });
  rows.push({
    key: "diagnostics.scanStrategy",
    value: strategy,
    source: "aecdatamodel",
  });
  rows.push({
    key: "diagnostics.pagesScanned",
    value: String(graphScan?.pagesScanned ?? 0),
    source: "aecdatamodel",
  });
  rows.push({
    key: "diagnostics.aecProjectId",
    value: aecProjectId,
    source: "aecdatamodel",
  });
  if (graphScan?.element) {
    const matchedElementId = String(graphScan.element.id ?? "");
    let elementForRows = graphScan.element;
    if (matchedElementId) {
      const fullProps = await fetchElementWithAllProperties(
        auth.session.accessToken,
        aecProjectId,
        matchedElementId,
      );
      rows.push({
        key: "diagnostics.fullPropertyPagesScanned",
        value: String(fullProps.pagesScanned),
        source: "aecdatamodel",
      });
      if (fullProps.element) {
        elementForRows = fullProps.element;
      }
    }
    rows.push(
      ...buildElementSchemaRows({
        element: elementForRows,
        requestDbId: dbId,
        requestElementName: elementName,
      }),
    );
  }

  if (!aec) {
    if (rows.length === 0) {
      rows.push({
        key: "aecdatamodel",
        value: "No AEC Data Model payload available for this model.",
        source: "aecdatamodel",
      });
    }
  } else {
    if (!graphScan?.element) {
      const matches = findMatchingObjects(aec.payload, externalId, dbId, 3);
      if (matches.length === 0) {
        rows.push({
          key: "match",
          value: "No direct AEC match found for selected element identifiers.",
          source: "aecdatamodel",
        });
      } else {
        matches.forEach((m, i) => {
          rows.push(
            ...flatten(m, `aecdatamodel:match[${i}]`, 120).slice(0, 120),
          );
        });
      }
    }
  }

  const response = NextResponse.json({
    rows: rows.slice(0, 300),
    rowCount: rows.length,
  });
  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}
