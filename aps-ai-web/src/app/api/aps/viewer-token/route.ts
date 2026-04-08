import { NextRequest, NextResponse } from "next/server";
import { requireSession } from "@/lib/auth-guard";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const auth = await requireSession(request);
  if (!auth.ok) return auth.response;

  const expiresIn = Math.max(
    1,
    Math.floor((auth.session.expiresAt - Date.now()) / 1000),
  );

  const response = NextResponse.json({
    access_token: auth.session.accessToken,
    token_type: "Bearer",
    expires_in: expiresIn,
  });

  for (const cookie of auth.response.cookies.getAll()) {
    response.cookies.set(cookie);
  }
  return response;
}

