import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { refreshApsToken, validateApsAccessToken } from "@/lib/aps";
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

function bearerAccessToken(request: NextRequest): string | null {
  const raw = request.headers.get("authorization");
  if (!raw) return null;
  const m = /^Bearer\s+(.+)$/i.exec(raw.trim());
  return m?.[1]?.trim() || null;
}

/**
 * Session from a validated `Authorization: Bearer` token (Monty iOS standalone),
 * or HttpOnly cookies that the server can refresh.
 */
export async function requireSession(request: NextRequest): Promise<AuthResult> {
  const bearer = bearerAccessToken(request);
  if (bearer) {
    try {
      await validateApsAccessToken(bearer);
      const response = NextResponse.next();
      return {
        ok: true,
        session: {
          accessToken: bearer,
          refreshToken: undefined,
          expiresAt: Number.MAX_SAFE_INTEGER,
          scope: "",
        },
        response,
      };
    } catch {
      // A stale native token should not mask a refreshable cookie session.
    }
  }

  const session = readSessionCookies(request);
  if (!session) {
    return {
      ok: false,
      response: NextResponse.json(
        { error: bearer ? "Invalid bearer token" : "Not authenticated" },
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
