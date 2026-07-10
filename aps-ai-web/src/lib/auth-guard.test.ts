import test from "node:test";
import assert from "node:assert/strict";
import { NextRequest } from "next/server";
import { requireSession } from "@/lib/auth-guard";

const originalFetch = globalThis.fetch;

function makeRequest(headers: HeadersInit): NextRequest {
  return new NextRequest("http://localhost/api/chat", { headers });
}

function mockFetch(response: Response): void {
  globalThis.fetch = (async () => response.clone()) as typeof fetch;
}

test.afterEach(() => {
  globalThis.fetch = originalFetch;
});

test("requireSession validates Authorization bearer tokens with APS", async () => {
  globalThis.fetch = (async (input, init) => {
    assert.equal(
      String(input),
      "https://developer.api.autodesk.com/userprofile/v1/users/@me",
    );
    assert.deepEqual(init?.headers, {
      Authorization: "Bearer valid-token",
      "Content-Type": "application/json",
    });
    return new Response(JSON.stringify({ userId: "me" }), { status: 200 });
  }) as typeof fetch;

  const auth = await requireSession(
    makeRequest({ authorization: "Bearer valid-token" }),
  );

  assert.equal(auth.ok, true);
  if (auth.ok) {
    assert.equal(auth.session.accessToken, "valid-token");
    assert.equal(auth.session.refreshToken, undefined);
  }
});

test("requireSession rejects invalid bearer tokens without cookies", async () => {
  mockFetch(new Response("Unauthorized", { status: 401 }));

  const auth = await requireSession(makeRequest({ authorization: "Bearer junk" }));

  assert.equal(auth.ok, false);
  if (!auth.ok) {
    assert.equal(auth.response.status, 401);
  }
});

test("requireSession falls back to cookies when bearer token is stale", async () => {
  mockFetch(new Response("Unauthorized", { status: 401 }));

  const expiresAt = Date.now() + 120_000;
  const auth = await requireSession(
    makeRequest({
      authorization: "Bearer stale-token",
      cookie: [
        "aps_access_token=cookie-token",
        `aps_expires_at=${expiresAt}`,
        "aps_scope=data:read",
      ].join("; "),
    }),
  );

  assert.equal(auth.ok, true);
  if (auth.ok) {
    assert.equal(auth.session.accessToken, "cookie-token");
    assert.equal(auth.session.expiresAt, expiresAt);
    assert.equal(auth.session.scope, "data:read");
  }
});
