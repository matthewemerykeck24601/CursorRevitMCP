import assert from "node:assert/strict";
import { createRequire } from "node:module";
import test from "node:test";

const require = createRequire(import.meta.url);
const { validateGatewaySharedSecret } = require("../lib/gateway-auth.js");

test("rejects requests when the configured gateway secret is missing", () => {
  assert.deepEqual(validateGatewaySharedSecret("", "caller-secret"), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });
});

test("rejects requests when the caller secret is missing or wrong", () => {
  assert.deepEqual(validateGatewaySharedSecret("expected-secret", ""), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateGatewaySharedSecret("expected-secret", "wrong-secret"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("accepts requests only when the caller secret matches", () => {
  assert.deepEqual(
    validateGatewaySharedSecret("expected-secret", "expected-secret"),
    { ok: true },
  );
});
