import { NextRequest, NextResponse } from "next/server";
import { exchangeAuthorizationCode } from "@/lib/aps";
import { env } from "@/lib/env";
import { getRequestId } from "@/lib/request";
import { writeSessionCookies } from "@/lib/session";
import { log } from "@/lib/logger";

export const runtime = "nodejs";

type Body = {
  code?: unknown;
  codeVerifier?: unknown;
  redirectUri?: unknown;
};

/**
 * Completes Monty iOS OAuth (PKCE): exchanges code for tokens using server secret,
 * returns Set-Cookie session compatible with other `aps-ai-web` routes.
 */
export async function POST(request: NextRequest) {
  const requestId = getRequestId(request);
  let body: Body;
  try {
    body = (await request.json()) as Body;
  } catch {
    return NextResponse.json(
      { error: "Invalid JSON body", requestId },
      { status: 400 },
    );
  }

  const code = typeof body.code === "string" ? body.code.trim() : "";
  const codeVerifier =
    typeof body.codeVerifier === "string" ? body.codeVerifier.trim() : "";
  const redirectUri =
    typeof body.redirectUri === "string" ? body.redirectUri.trim() : "";

  if (!code || !codeVerifier) {
    return NextResponse.json(
      { error: "code and codeVerifier are required", requestId },
      { status: 400 },
    );
  }

  if (redirectUri !== env.apsNativeRedirectUri) {
    return NextResponse.json(
      { error: "redirectUri does not match APS_NATIVE_REDIRECT_URI", requestId },
      { status: 400 },
    );
  }

  try {
    const token = await exchangeAuthorizationCode({
      code,
      redirectUri,
      codeVerifier,
    });

    const response = NextResponse.json({
      requestId,
      ok: true,
      expiresAt: Date.now() + token.expires_in * 1000,
      expiresIn: token.expires_in,
      scope: token.scope ?? "",
      accessToken: token.access_token,
      refreshToken: token.refresh_token ?? "",
    });

    writeSessionCookies(response, {
      accessToken: token.access_token,
      refreshToken: token.refresh_token,
      expiresAt: Date.now() + token.expires_in * 1000,
      scope: token.scope,
    });

    return response;
  } catch (error) {
    log("warn", "native-exchange-failed", {
      requestId,
      details: error instanceof Error ? error.message : "unknown",
    });
    return NextResponse.json(
      {
        error: error instanceof Error ? error.message : "Exchange failed",
        requestId,
      },
      { status: 400 },
    );
  }
}
