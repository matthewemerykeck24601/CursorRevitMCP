import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { syncLookupTablesFromHubCaches } from "@/lib/admin-lookup-sync";
import { env } from "@/lib/env";

export const runtime = "nodejs";

type IngestRequest = {
  tenantId?: string;
  hubId?: string;
  region?: "US" | "EMEA";
  refreshReferenceCache?: boolean;
};

function normalizeRegion(v: unknown): "US" | "EMEA" {
  return String(v ?? "US").toUpperCase() === "EMEA" ? "EMEA" : "US";
}

function validateIngestSecret(request: NextRequest): NextResponse | null {
  const expectedSecret = env.adminLookupIngestSecret.trim();
  if (!expectedSecret) {
    return NextResponse.json(
      { success: false, error: "Admin lookup ingest is not configured." },
      { status: 500 },
    );
  }

  const receivedSecret =
    request.headers.get("x-admin-lookup-ingest-secret")?.trim() ?? "";
  if (!receivedSecret || receivedSecret !== expectedSecret) {
    return NextResponse.json(
      { success: false, error: "Unauthorized admin lookup ingest request." },
      { status: 403 },
    );
  }

  return null;
}

export async function POST(request: NextRequest) {
  const secretError = validateIngestSecret(request);
  if (secretError) return secretError;

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
