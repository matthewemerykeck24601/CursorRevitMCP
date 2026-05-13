const assert = require("node:assert/strict");
const test = require("node:test");

const { validateGatewaySecret } = require("../lib/gateway-auth.js");

test("rejects every request when the gateway secret is missing", () => {
  assert.deepEqual(validateGatewaySecret("", ""), {
    ok: false,
    status: 503,
    error: "AI gateway shared secret is not configured.",
  });
  assert.deepEqual(validateGatewaySecret("   ", "anything"), {
    ok: false,
    status: 503,
    error: "AI gateway shared secret is not configured.",
  });
});

test("rejects missing or mismatched gateway secret headers", () => {
  assert.deepEqual(validateGatewaySecret("expected", ""), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateGatewaySecret("expected", "wrong"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("accepts an exact matching gateway secret header", () => {
  assert.deepEqual(validateGatewaySecret("expected", "expected"), { ok: true });
});
