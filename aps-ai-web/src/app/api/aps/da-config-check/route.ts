import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getRevitCloudModelInfo } from "@/lib/aps";
import { env } from "@/lib/env";
import { resolveDaActivityIdFromWorkitemArgs } from "@/lib/da-activity-routing";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const projectId = request.nextUrl.searchParams.get("projectId")?.trim() ?? "";
  const versionId = request.nextUrl.searchParams.get("versionId")?.trim() ?? "";

  let cloudModel: Awaited<ReturnType<typeof getRevitCloudModelInfo>> = null;
  if (projectId && versionId) {
    try {
      cloudModel = await getRevitCloudModelInfo({
        accessToken: auth.session.accessToken,
        projectId,
        versionId,
      });
    } catch {
      // Keep endpoint diagnostic-only; return activity resolution with available env fallbacks.
      cloudModel = null;
    }
  }

  let resolved: { activityId: string; source: string; revitVersionMajor?: number } | null = null;
  let resolveError = "";
  try {
    resolved = resolveDaActivityIdFromWorkitemArgs(
      cloudModel ? { cloud_model: cloudModel } : {},
    );
  } catch (error) {
    resolveError = error instanceof Error ? error.message : "Unable to resolve DA activity";
  }

  const response = NextResponse.json({
    success: true,
    da_enabled: env.daEnabled === "true",
    region: env.daRegion || "us-east",
    projectId: projectId || null,
    versionId: versionId || null,
    cloud_model: cloudModel,
    resolved_activity: resolved,
    resolve_error: resolveError || null,
    configured_activity_ids: {
      DA_ACTIVITY_ID: env.daActivityId || null,
      DA_ACTIVITY_ID_2024: env.daActivityId2024 || null,
      DA_ACTIVITY_ID_2025: env.daActivityId2025 || null,
      DA_ACTIVITY_ID_2026: env.daActivityId2026 || null,
      DA_ACTIVITY_ID_2027: env.daActivityId2027 || null,
      DA_ACTIVITY_ID_NET8: env.daActivityIdNet8 || null,
      DA_ACTIVITY_ID_NET10: env.daActivityIdNet10 || null,
    },
  });
  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}
