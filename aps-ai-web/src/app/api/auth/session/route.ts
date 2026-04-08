import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";
import { getRequestId } from "@/lib/request";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const requestId = getRequestId(request);
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const response = NextResponse.json({
    requestId,
    authenticated: true,
    expiresAt: auth.session.expiresAt,
    scope: auth.session.scope ?? "",
  });

  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}

