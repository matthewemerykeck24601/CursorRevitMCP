const test = require("node:test");
const assert = require("node:assert/strict");

const { validateGatewaySecret } = require("../lib/gateway-auth.js");

test("rejects requests when the gateway shared secret is not configured", () => {
  assert.deepEqual(validateGatewaySecret("", "client-secret"), {
    ok: false,
    status: 500,
    error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
  });
});

test("rejects missing or mismatched gateway shared secrets", () => {
  assert.deepEqual(validateGatewaySecret("server-secret", undefined), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateGatewaySecret("server-secret", "wrong-secret"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("accepts matching gateway shared secrets", () => {
  assert.deepEqual(validateGatewaySecret("server-secret", "server-secret"), {
    ok: true,
  });
});
