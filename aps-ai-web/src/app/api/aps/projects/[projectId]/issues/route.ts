import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import {
  createProjectIssue,
  getIssuesContainerId,
  listProjectIssues,
} from "@/lib/aps-issues";

export const runtime = "nodejs";

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { projectId } = await context.params;
  const containerId =
    request.nextUrl.searchParams.get("containerId") ?? getIssuesContainerId();
  if (!containerId) {
    return NextResponse.json(
      {
        error:
          "Issues container is not configured. Set APS_ISSUES_CONTAINER_ID or pass containerId query param.",
        projectId,
      },
      { status: 400 },
    );
  }

  try {
    const issues = await listProjectIssues(auth.session.accessToken, containerId);
    const response = NextResponse.json({
      projectId,
      containerId,
      issues,
      count: issues.length,
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: "Failed to list issues",
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}

export async function POST(
  request: NextRequest,
  context: { params: Promise<{ projectId: string }> },
) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const { projectId } = await context.params;
  const body = (await request.json()) as {
    title?: string;
    description?: string;
    containerId?: string;
  };
  const title = (body.title ?? "").trim();
  if (!title) {
    return NextResponse.json({ error: "Missing required field: title" }, { status: 400 });
  }
  const containerId = body.containerId ?? getIssuesContainerId();
  if (!containerId) {
    return NextResponse.json(
      {
        error:
          "Issues container is not configured. Set APS_ISSUES_CONTAINER_ID or pass containerId in body.",
        projectId,
      },
      { status: 400 },
    );
  }

  try {
    const issue = await createProjectIssue(auth.session.accessToken, containerId, {
      title,
      description: body.description,
    });
    const response = NextResponse.json({
      projectId,
      containerId,
      issue,
    });
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  } catch (error) {
    return NextResponse.json(
      {
        error: "Failed to create issue",
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 500 },
    );
  }
}
