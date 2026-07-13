import test from "node:test";
import assert from "node:assert/strict";
import { NextRequest } from "next/server";
import { requireSession } from "@/lib/auth-guard";

test("requireSession rejects invalid bearer tokens", async (t) => {
  const originalFetch = globalThis.fetch;
  let validationCalls = 0;
  globalThis.fetch = (async (input) => {
    validationCalls += 1;
    assert.match(String(input), /\/userprofile\/v1\/users\/@me$/);
    return new Response("invalid", { status: 401 });
  }) as typeof fetch;
  t.after(() => {
    globalThis.fetch = originalFetch;
  });

  const result = await requireSession(
    new NextRequest("http://localhost/api/chat", {
      headers: { authorization: "Bearer not-a-real-token" },
    }),
  );

  assert.equal(result.ok, false);
  assert.equal(result.response.status, 401);
  assert.equal(validationCalls, 1);
});

test("requireSession accepts validated bearer tokens", async (t) => {
  const originalFetch = globalThis.fetch;
  globalThis.fetch = (async (input) => {
    assert.match(String(input), /\/userprofile\/v1\/users\/@me$/);
    return Response.json({ userId: "test-user" });
  }) as typeof fetch;
  t.after(() => {
    globalThis.fetch = originalFetch;
  });

  const result = await requireSession(
    new NextRequest("http://localhost/api/chat", {
      headers: { authorization: "Bearer real-token" },
    }),
  );

  assert.equal(result.ok, true);
  if (result.ok) {
    assert.equal(result.session.accessToken, "real-token");
    assert.equal(result.session.refreshToken, undefined);
  }
});
