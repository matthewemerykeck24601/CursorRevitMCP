import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getRequestId } from "@/lib/request";
import { log } from "@/lib/logger";

type ModelWriteAction = "create" | "edit" | "delete";

type ModelWriteRequest = {
  action: ModelWriteAction;
  target?: Record<string, unknown>;
  payload?: Record<string, unknown>;
};

export const runtime = "nodejs";

function writeActionsEnabled(): boolean {
  return process.env.ENABLE_MODEL_WRITE_ACTIONS === "true";
}

export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const body = (await request.json()) as ModelWriteRequest;
  if (!body.action || !["create", "edit", "delete"].includes(body.action)) {
    return NextResponse.json(
      { error: "Invalid action. Expected create|edit|delete", requestId },
      { status: 400 },
    );
  }

  if (!writeActionsEnabled()) {
    log("warn", "model-write-blocked-feature-flag", {
      requestId,
      action: body.action,
    });
    const response = NextResponse.json(
      {
        ok: false,
        requestId,
        action: body.action,
        message:
          "Write actions are disabled in Phase 1. Set ENABLE_MODEL_WRITE_ACTIONS=true to enable future implementations.",
      },
      { status: 403 },
    );
    for (const cookie of auth.response.cookies.getAll()) {
      response.cookies.set(cookie);
    }
    return response;
  }

  // Contract exists in Phase 1. Actual write implementations come in Phase 2.
  const response = NextResponse.json(
    {
      ok: false,
      requestId,
      action: body.action,
      message:
        "Write action contract accepted, but concrete implementation is deferred to Phase 2.",
    },
    { status: 501 },
  );
  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}

