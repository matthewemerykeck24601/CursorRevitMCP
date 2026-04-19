import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { addUsersToProjectsByNumber } from "@/lib/aps-admin";
import { getRequestId } from "@/lib/request";
import { log } from "@/lib/logger";

export const runtime = "nodejs";

type Body = {
  hubId?: unknown;
  projectNumbers?: unknown;
  emails?: unknown;
  roleNames?: unknown;
  roleIds?: unknown;
  region?: unknown;
  dryRun?: unknown;
  cacheOnly?: unknown;
  businessUnitId?: unknown;
  businessUnitName?: unknown;
};

function asStringArray(v: unknown): string[] {
  if (!Array.isArray(v)) return [];
  return v.map((x) => String(x).trim()).filter(Boolean);
}

/**
 * Direct ACC admin: add users to projects by project number (same backend as chat tool `admin_add_users_to_projects`).
 * Used by Monty iOS and other structured clients — no MCP or LLM required.
 */
export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  let body: Body;
  try {
    body = (await request.json()) as Body;
  } catch {
    return NextResponse.json(
      { error: "Invalid JSON body", requestId },
      { status: 400 },
    );
  }

  const hubId = typeof body.hubId === "string" ? body.hubId.trim() : "";
  if (!hubId) {
    return NextResponse.json(
      { error: "hubId is required", requestId },
      { status: 400 },
    );
  }

  const projectNumbers = asStringArray(body.projectNumbers);
  const emails = asStringArray(body.emails);
  const roleNames = asStringArray(body.roleNames);
  const roleIds = asStringArray(body.roleIds);
  const businessUnitId =
    typeof body.businessUnitId === "string" ? body.businessUnitId.trim() : "";
  const businessUnitName =
    typeof body.businessUnitName === "string" ? body.businessUnitName.trim() : "";
  const regionRaw = String(body.region ?? "US").toUpperCase();
  const region = regionRaw === "EMEA" ? "EMEA" : "US";
  const dryRun = Boolean(body.dryRun);
  const cacheOnlyArg = body.cacheOnly;
  const preferCacheOnly =
    cacheOnlyArg == null ? projectNumbers.length >= 5 : Boolean(cacheOnlyArg);

  try {
    const result = await addUsersToProjectsByNumber({
      accessToken: auth.session.accessToken,
      hubId,
      projectNumbers,
      businessUnitId: businessUnitId || undefined,
      businessUnitName: businessUnitName || undefined,
      emails,
      roleIds,
      roleNames,
      region,
      dryRun,
      cacheOnly: preferCacheOnly,
    });

    const response = NextResponse.json({ requestId, ...result });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    log("warn", "admin-add-users-to-projects-failed", {
      requestId,
      details: error instanceof Error ? error.message : "unknown",
    });
    return NextResponse.json(
      {
        error: error instanceof Error ? error.message : "Request failed",
        requestId,
      },
      { status: 400 },
    );
  }
}
