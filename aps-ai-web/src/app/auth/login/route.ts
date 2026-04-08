import { randomUUID } from "node:crypto";
import { NextResponse } from "next/server";
import { buildAuthorizeUrl } from "@/lib/aps";
import { writeOAuthStateCookie } from "@/lib/session";

export const runtime = "nodejs";

export async function GET() {
  const state = randomUUID();
  const response = NextResponse.redirect(buildAuthorizeUrl(state));
  writeOAuthStateCookie(response, state);
  return response;
}

