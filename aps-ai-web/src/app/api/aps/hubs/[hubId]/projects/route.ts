import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { listHubProjects, upsertHubReferenceProjects } from "@/lib/aps-admin";
import { getRequestId } from "@/lib/request";
import { log } from "@/lib/logger";

export const runtime = "nodejs";

const projectNameCollator = new Intl.Collator(undefined, {
  numeric: true,
  sensitivity: "base",
});

function startsWithDigit(value: string): boolean {
  return /^\d/.test(value.trim());
}

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ hubId: string }> },
) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { hubId } = await context.params;

  try {
    const projects = await listHubProjects(
      auth.session.accessToken,
      hubId,
    );
    try {
      await upsertHubReferenceProjects({
        hubId,
        projects,
      });
    } catch (error) {
      log("warn", "aps-projects-cache-upsert-failed", {
        requestId,
        hubId,
        details: error instanceof Error ? error.message : "Unknown error",
      });
    }

    const orderedProjects = projects
      .map((project) => ({
        id: project.projectId,
        name: project.projectName || project.projectId,
        type: "autodesk.bim360:Project",
      }))
      .sort((a, b) => {
        const aNumeric = startsWithDigit(a.name);
        const bNumeric = startsWithDigit(b.name);
        if (aNumeric !== bNumeric) return aNumeric ? -1 : 1;
        return projectNameCollator.compare(a.name, b.name);
      });

    const response = NextResponse.json({
      requestId,
      projects: orderedProjects,
    });

    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    log("error", "aps-projects-failed", {
      requestId,
      details: error instanceof Error ? error.message : "Unknown error",
    });
    return NextResponse.json(
      {
        error: "Failed to fetch projects",
        requestId,
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}

