import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";

const ACCESS_TOKEN_COOKIE = "aps_access_token";
const REFRESH_TOKEN_COOKIE = "aps_refresh_token";
const EXPIRES_AT_COOKIE = "aps_expires_at";
const SCOPE_COOKIE = "aps_scope";
const OAUTH_STATE_COOKIE = "aps_oauth_state";

const secure = process.env.NODE_ENV === "production";

function cookieOptions(maxAgeSeconds: number) {
  return {
    httpOnly: true,
    sameSite: "lax" as const,
    secure,
    path: "/",
    maxAge: maxAgeSeconds,
  };
}

export type UserSession = {
  accessToken: string;
  refreshToken?: string;
  expiresAt: number;
  scope?: string;
};

export function writeSessionCookies(
  response: NextResponse,
  session: UserSession,
): void {
  const ttl = Math.max(1, Math.floor((session.expiresAt - Date.now()) / 1000));
  response.cookies.set(
    ACCESS_TOKEN_COOKIE,
    session.accessToken,
    cookieOptions(ttl),
  );

  if (session.refreshToken) {
    response.cookies.set(
      REFRESH_TOKEN_COOKIE,
      session.refreshToken,
      cookieOptions(60 * 60 * 24 * 30),
    );
  }

  response.cookies.set(
    EXPIRES_AT_COOKIE,
    String(session.expiresAt),
    cookieOptions(ttl),
  );
  response.cookies.set(SCOPE_COOKIE, session.scope ?? "", cookieOptions(ttl));
}

export function readSessionCookies(request: NextRequest): UserSession | null {
  const accessToken = request.cookies.get(ACCESS_TOKEN_COOKIE)?.value;
  const expiresAtRaw = request.cookies.get(EXPIRES_AT_COOKIE)?.value;
  if (!accessToken || !expiresAtRaw) {
    return null;
  }

  const expiresAt = Number(expiresAtRaw);
  if (!Number.isFinite(expiresAt)) {
    return null;
  }

  return {
    accessToken,
    refreshToken: request.cookies.get(REFRESH_TOKEN_COOKIE)?.value,
    expiresAt,
    scope: request.cookies.get(SCOPE_COOKIE)?.value,
  };
}

export function clearSessionCookies(response: NextResponse): void {
  response.cookies.set(ACCESS_TOKEN_COOKIE, "", cookieOptions(0));
  response.cookies.set(REFRESH_TOKEN_COOKIE, "", cookieOptions(0));
  response.cookies.set(EXPIRES_AT_COOKIE, "", cookieOptions(0));
  response.cookies.set(SCOPE_COOKIE, "", cookieOptions(0));
}

export function writeOAuthStateCookie(
  response: NextResponse,
  state: string,
): void {
  response.cookies.set(OAUTH_STATE_COOKIE, state, cookieOptions(600));
}

export function readOAuthStateCookie(request: NextRequest): string | null {
  return request.cookies.get(OAUTH_STATE_COOKIE)?.value ?? null;
}

export function clearOAuthStateCookie(response: NextResponse): void {
  response.cookies.set(OAUTH_STATE_COOKIE, "", cookieOptions(0));
}

