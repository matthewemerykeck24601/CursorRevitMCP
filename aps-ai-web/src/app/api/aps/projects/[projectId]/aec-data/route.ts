import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { apsGet } from "@/lib/aps";

export const runtime = "nodejs";

type KeyValueRow = {
  key: string;
  value: string;
  source: string;
};

function flattenToRows(
  input: unknown,
  source: string,
  maxRows = 200,
): KeyValueRow[] {
  const rows: KeyValueRow[] = [];
  const walk = (value: unknown, path: string) => {
    if (rows.length >= maxRows) return;
    if (value === null || value === undefined) {
      rows.push({ key: path, value: String(value), source });
      return;
    }
    if (typeof value !== "object") {
      rows.push({ key: path, value: String(value), source });
      return;
    }
    if (Array.isArray(value)) {
      if (value.length === 0) {
        rows.push({ key: path, value: "[]", source });
        return;
      }
      value.forEach((v, i) => walk(v, `${path}[${i}]`));
      return;
    }
    for (const [k, v] of Object.entries(value as Record<string, unknown>)) {
      walk(v, path ? `${path}.${k}` : k);
      if (rows.length >= maxRows) break;
    }
  };
  walk(input, "");
  return rows;
}

async function tryAecDataCalls(
  accessToken: string,
  hubId: string,
  projectId: string,
  itemId?: string,
): Promise<{ source: string; payload: unknown } | null> {
  const candidates: string[] = [];
  if (itemId) {
    candidates.push(
      `/aecdatamodel/v1/hubs/${encodeURIComponent(hubId)}/projects/${encodeURIComponent(projectId)}/items/${encodeURIComponent(itemId)}`,
    );
  }
  candidates.push(
    `/aecdatamodel/v1/hubs/${encodeURIComponent(hubId)}/projects/${encodeURIComponent(projectId)}`,
  );

  for (const path of candidates) {
    try {
      const json = await apsGet<unknown>(path, accessToken);
      return { source: `aecdatamodel:${path}`, payload: json };
    } catch {
      // Try next candidate.
    }
  }
  return null;
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
  const versionId = request.nextUrl.searchParams.get("versionId") ?? undefined;
  const viewerUrn = request.nextUrl.searchParams.get("viewerUrn") ?? undefined;

  if (!hubId) {
    return NextResponse.json(
      { error: "Missing required query parameter: hubId" },
      { status: 400 },
    );
  }

  const rows: KeyValueRow[] = [
    { key: "hubId", value: hubId, source: "request" },
    { key: "projectId", value: projectId, source: "request" },
    ...(itemId ? [{ key: "itemId", value: itemId, source: "request" }] : []),
    ...(versionId
      ? [{ key: "versionId", value: versionId, source: "request" }]
      : []),
  ];

  try {
    const aec = await tryAecDataCalls(
      auth.session.accessToken,
      hubId,
      projectId,
      itemId,
    );
    if (aec) {
      rows.push(...flattenToRows(aec.payload, aec.source, 240));
    } else {
      rows.push({
        key: "aecdatamodel",
        value:
          "No AEC Data Model payload available from tried endpoints for this model/project.",
        source: "aecdatamodel",
      });
    }

    if (viewerUrn) {
      try {
        const metadata = await apsGet<unknown>(
          `/modelderivative/v2/designdata/${encodeURIComponent(viewerUrn)}/metadata`,
          auth.session.accessToken,
        );
        rows.push(...flattenToRows(metadata, "modelderivative:metadata", 120));
      } catch {
        rows.push({
          key: "modelderivative.metadata",
          value: "Metadata endpoint unavailable for supplied viewerUrn.",
          source: "modelderivative",
        });
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
  } catch (error) {
    return NextResponse.json(
      {
        error: "Failed to load model data",
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}
