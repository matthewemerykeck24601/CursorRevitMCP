import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { queryAecDataModel } from "@/lib/aps";

export const runtime = "nodejs";

type DataRow = {
  key: string;
  value: string;
  source: string;
};

function flatten(input: unknown, source: string, limit = 300): DataRow[] {
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
      if (v.length === 0) {
        out.push({ key: path, value: "[]", source });
        return;
      }
      v.forEach((item, idx) => walk(item, `${path}[${idx}]`));
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

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { projectId } = await context.params;
  const hubId = request.nextUrl.searchParams.get("hubId");
  const itemId = request.nextUrl.searchParams.get("itemId") ?? undefined;

  if (!hubId) {
    return NextResponse.json(
      { error: "Missing required query parameter: hubId" },
      { status: 400 },
    );
  }

  const rows: DataRow[] = [
    { key: "hubId", value: hubId, source: "request" },
    { key: "projectId", value: projectId, source: "request" },
    ...(itemId ? [{ key: "itemId", value: itemId, source: "request" }] : []),
  ];

  const aec = await queryAecDataModel(
    auth.session.accessToken,
    hubId,
    projectId,
    itemId,
  );
  if (!aec) {
    rows.push({
      key: "aecdatamodel",
      value: "No AEC Data Model payload available for this context.",
      source: "aecdatamodel",
    });
  } else {
    rows.push(...flatten(aec.payload, `aecdatamodel:${aec.sourcePath}`, 300));
  }

  const response = NextResponse.json({ rows, rowCount: rows.length });
  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}
