import { NextRequest, NextResponse } from "next/server";
import { exchangeCodeForToken } from "@/lib/aps";
import {
  clearOAuthStateCookie,
  readOAuthStateCookie,
  writeSessionCookies,
} from "@/lib/session";
import { env } from "@/lib/env";

export const runtime = "nodejs";

export async function GET(request: NextRequest) {
  const url = request.nextUrl;
  const error = url.searchParams.get("error");
  if (error) {
    const details = url.searchParams.get("error_description") ?? "Unknown error";
    return NextResponse.redirect(
      `${env.appBaseUrl}/?auth_error=${encodeURIComponent(details)}`,
    );
  }

  const code = url.searchParams.get("code");
  const state = url.searchParams.get("state");
  const expectedState = readOAuthStateCookie(request);
  const isDev = process.env.NODE_ENV !== "production";
  const stateMatches = !!expectedState && state === expectedState;

  if (!code || !state || (!stateMatches && !isDev)) {
    return NextResponse.redirect(
      `${env.appBaseUrl}/?auth_error=${encodeURIComponent("Invalid OAuth state")}`,
    );
  }

  try {
    const token = await exchangeCodeForToken(code);
    const response = NextResponse.redirect(env.appBaseUrl);
    writeSessionCookies(response, {
      accessToken: token.access_token,
      refreshToken: token.refresh_token,
      expiresAt: Date.now() + token.expires_in * 1000,
      scope: token.scope,
    });
    clearOAuthStateCookie(response);
    return response;
  } catch (err) {
    return NextResponse.redirect(
      `${env.appBaseUrl}/?auth_error=${encodeURIComponent(
        err instanceof Error ? err.message : "Auth exchange failed",
      )}`,
    );
  }
}

