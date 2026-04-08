import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsPost, queryAecDataModel, resolveAecProjectId } from "@/lib/aps";

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

type AecElementScanResponse = {
  data?: {
    elementsByProject?: {
      pagination?: { cursor?: string | null };
      results?: GraphElement[];
    };
  };
};

async function fetchElementByExternalIdPaged(
  accessToken: string,
  aecProjectId: string,
  externalId: string,
): Promise<{ element: GraphElement | null; pagesScanned: number }> {
  if (!externalId) return { element: null, pagesScanned: 0 };
  const target = externalId.toLowerCase();
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
          properties(filter: { names: ["External ID", "Element Id", "Element ID", "CONTROL_MARK", "CONTROL_NUMBER", "Category", "Family Name", "Type Name", "Length", "Width", "Height"] }) {
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

  let cursor: string | null = null;
  let pagesScanned = 0;
  for (let i = 0; i < 60; i += 1) {
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
      const props = element.properties?.results ?? [];
      const extValue = props.find(
        (p) => String(p.name ?? "").toLowerCase() === "external id",
      )?.value;
      if (String(extValue ?? "").toLowerCase() === target) {
        return { element, pagesScanned };
      }
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
  const graphScan = await fetchElementByExternalIdPaged(
    auth.session.accessToken,
    aecProjectId,
    externalId,
  );
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
    rows.push(
      ...flatten(
        graphScan.element,
        "aecdatamodel:/aec/graphql#elementByExternalId",
        240,
      ),
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
