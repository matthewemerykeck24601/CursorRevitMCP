import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { syncLookupTablesFromHubCaches } from "@/lib/admin-lookup-sync";

type IngestRequest = {
  tenantId?: string;
  hubId?: string;
  region?: "US" | "EMEA";
  refreshReferenceCache?: boolean;
};

function normalizeRegion(v: unknown): "US" | "EMEA" {
  return String(v ?? "US").toUpperCase() === "EMEA" ? "EMEA" : "US";
}

export async function POST(request: NextRequest) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  let body: IngestRequest = {};
  try {
    body = (await request.json()) as IngestRequest;
  } catch {
    body = {};
  }

  const tenantId = String(body.tenantId ?? "").trim();
  const hubId = String(body.hubId ?? "").trim();
  if (!tenantId || !hubId) {
    return NextResponse.json(
      { success: false, error: "tenantId and hubId are required." },
      { status: 400 },
    );
  }

  try {
    const result = await syncLookupTablesFromHubCaches({
      tenantId,
      hubId,
      accessToken: auth.session.accessToken,
      region: normalizeRegion(body.region),
      refreshReferenceCache: Boolean(body.refreshReferenceCache),
    });
    const status = result.success ? 200 : 409;
    return NextResponse.json(result, { status });
  } catch (error) {
    return NextResponse.json(
      {
        success: false,
        error: error instanceof Error ? error.message : "Unknown ingest error",
      },
      { status: 500 },
    );
  }
}
