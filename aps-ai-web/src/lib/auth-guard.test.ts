import test from "node:test";
import assert from "node:assert/strict";
import { NextRequest } from "next/server";
import { requireSession } from "@/lib/auth-guard";

const APS_USER_PROFILE_URL =
  "https://developer.api.autodesk.com/userprofile/v1/users/@me";

function bearerRequest(token: string): NextRequest {
  return new NextRequest("http://localhost/api/test", {
    headers: { authorization: `Bearer ${token}` },
  });
}

test("requireSession rejects bearer tokens that APS does not validate", async () => {
  const originalFetch = globalThis.fetch;
  try {
    globalThis.fetch = (async (input, init) => {
      assert.equal(input, APS_USER_PROFILE_URL);
      assert.equal(
        (init?.headers as Record<string, string>).Authorization,
        "Bearer forged-token",
      );
      return new Response("unauthorized", { status: 401 });
    }) as typeof fetch;

    const result = await requireSession(bearerRequest("forged-token"));

    assert.equal(result.ok, false);
    assert.equal(result.response.status, 401);
  } finally {
    globalThis.fetch = originalFetch;
  }
});

test("requireSession accepts bearer tokens that APS validates", async () => {
  const originalFetch = globalThis.fetch;
  try {
    globalThis.fetch = (async (input, init) => {
      assert.equal(input, APS_USER_PROFILE_URL);
      assert.equal(
        (init?.headers as Record<string, string>).Authorization,
        "Bearer valid-token",
      );
      return Response.json({ userId: "user-1" });
    }) as typeof fetch;

    const result = await requireSession(bearerRequest("valid-token"));

    assert.equal(result.ok, true);
    if (result.ok) {
      assert.equal(result.session.accessToken, "valid-token");
      assert.equal(Number.isFinite(result.session.expiresAt), true);
    }
  } finally {
    globalThis.fetch = originalFetch;
  }
});

