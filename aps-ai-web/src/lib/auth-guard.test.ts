import test from "node:test";
import assert from "node:assert/strict";
import { NextRequest } from "next/server";
import { requireSession } from "@/lib/auth-guard";

const originalFetch = globalThis.fetch;

test.afterEach(() => {
  globalThis.fetch = originalFetch;
});

test("requireSession rejects bearer tokens that APS does not accept", async () => {
  globalThis.fetch = async () => new Response("Unauthorized", { status: 401 });

  const result = await requireSession(
    new NextRequest("http://localhost/api/chat", {
      headers: { authorization: "Bearer not-a-real-aps-token" },
    }),
  );

  assert.equal(result.ok, false);
  assert.equal(result.response.status, 401);
});

test("requireSession accepts bearer tokens validated by APS", async () => {
  let requestedAuthorization = "";
  globalThis.fetch = async (_input, init) => {
    requestedAuthorization = String(
      (init?.headers as Record<string, string> | undefined)?.Authorization ?? "",
    );
    return Response.json({ userId: "user" });
  };

  const result = await requireSession(
    new NextRequest("http://localhost/api/chat", {
      headers: { authorization: "Bearer aps-token" },
    }),
  );

  assert.equal(result.ok, true);
  if (result.ok) {
    assert.equal(result.session.accessToken, "aps-token");
  }
  assert.equal(requestedAuthorization, "Bearer aps-token");
});
