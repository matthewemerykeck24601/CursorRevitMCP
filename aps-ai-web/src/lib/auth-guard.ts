import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { refreshApsToken } from "@/lib/aps";
import {
  clearSessionCookies,
  readSessionCookies,
  type UserSession,
  writeSessionCookies,
} from "@/lib/session";

const REFRESH_BUFFER_MS = 60_000;

type AuthResult =
  | { ok: true; session: UserSession; response: NextResponse }
  | { ok: false; response: NextResponse };

export async function requireSession(request: NextRequest): Promise<AuthResult> {
  const session = readSessionCookies(request);
  if (!session) {
    return {
      ok: false,
      response: NextResponse.json(
        { error: "Not authenticated" },
        { status: 401 },
      ),
    };
  }

  const response = NextResponse.next();
  if (Date.now() < session.expiresAt - REFRESH_BUFFER_MS) {
    return { ok: true, session, response };
  }

  if (!session.refreshToken) {
    const unauthorized = NextResponse.json(
      { error: "Session expired, please login again" },
      { status: 401 },
    );
    clearSessionCookies(unauthorized);
    return { ok: false, response: unauthorized };
  }

  try {
    const refreshed = await refreshApsToken(session.refreshToken);
    const nextSession: UserSession = {
      accessToken: refreshed.access_token,
      refreshToken: refreshed.refresh_token ?? session.refreshToken,
      expiresAt: Date.now() + refreshed.expires_in * 1000,
      scope: refreshed.scope ?? session.scope,
    };
    writeSessionCookies(response, nextSession);
    return { ok: true, session: nextSession, response };
  } catch (error) {
    const unauthorized = NextResponse.json(
      {
        error: "Failed to refresh session",
        details: error instanceof Error ? error.message : "Unknown error",
      },
      { status: 401 },
    );
    clearSessionCookies(unauthorized);
    return { ok: false, response: unauthorized };
  }
}

