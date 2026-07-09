import test from "node:test";
import assert from "node:assert/strict";
import { NextRequest } from "next/server";
import { requireSession } from "@/lib/auth-guard";

const originalFetch = globalThis.fetch;

function mockFetch(response: Response): void {
  globalThis.fetch = (async () => response) as typeof fetch;
}

test.afterEach(() => {
  globalThis.fetch = originalFetch;
});

test("requireSession rejects an invalid bearer without cookies", async () => {
  mockFetch(new Response("invalid", { status: 401 }));

  const request = new NextRequest("https://example.test/api/chat", {
    headers: { authorization: "Bearer not-a-token" },
  });

  const auth = await requireSession(request);

  assert.equal(auth.ok, false);
  assert.equal(auth.response.status, 401);
  assert.deepEqual(await auth.response.json(), {
    error: "Invalid bearer token",
  });
});

test("requireSession accepts a bearer only after APS validation", async () => {
  let authorization = "";
  globalThis.fetch = (async (_input, init) => {
    authorization = new Headers(init?.headers).get("Authorization") ?? "";
    return new Response(JSON.stringify({ userId: "user-1" }), { status: 200 });
  }) as typeof fetch;

  const request = new NextRequest("https://example.test/api/chat", {
    headers: { authorization: "Bearer valid-token" },
  });

  const auth = await requireSession(request);

  assert.equal(auth.ok, true);
  if (auth.ok) {
    assert.equal(auth.session.accessToken, "valid-token");
    assert.equal(auth.session.refreshToken, undefined);
  }
  assert.equal(authorization, "Bearer valid-token");
});

test("requireSession falls back to cookies when a native bearer is stale", async () => {
  mockFetch(new Response("expired", { status: 401 }));
  const expiresAt = Date.now() + 10 * 60 * 1000;
  const cookie = [
    "aps_access_token=cookie-token",
    `aps_expires_at=${expiresAt}`,
    "aps_scope=user%3Aread",
  ].join("; ");
  const request = new NextRequest("https://example.test/api/chat", {
    headers: {
      authorization: "Bearer expired-token",
      cookie,
    },
  });

  const auth = await requireSession(request);

  assert.equal(auth.ok, true);
  if (auth.ok) {
    assert.equal(auth.session.accessToken, "cookie-token");
    assert.equal(auth.session.scope, "user:read");
  }
});
