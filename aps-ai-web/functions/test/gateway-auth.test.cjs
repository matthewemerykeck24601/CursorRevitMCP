const assert = require("node:assert/strict");
const test = require("node:test");

const { validateGatewaySharedSecret } = require("../lib/gateway-auth");

test("rejects gateway requests when the configured shared secret is missing", () => {
  assert.deepEqual(validateGatewaySharedSecret("", "provided"), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });
  assert.deepEqual(validateGatewaySharedSecret(undefined, "provided"), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });
});

test("rejects gateway requests with a missing or wrong header", () => {
  assert.deepEqual(validateGatewaySharedSecret("expected", ""), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateGatewaySharedSecret("expected", "wrong"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("accepts gateway requests with the configured shared secret", () => {
  assert.deepEqual(validateGatewaySharedSecret("expected", "expected"), { ok: true });
});
