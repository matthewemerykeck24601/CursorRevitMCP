const assert = require("node:assert/strict");
const test = require("node:test");

const { validateGatewaySharedSecret } = require("../lib/auth");

test("validateGatewaySharedSecret fails closed when expected secret is missing", () => {
  assert.deepEqual(validateGatewaySharedSecret("", "sent-secret"), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });
});

test("validateGatewaySharedSecret rejects missing or mismatched request secrets", () => {
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

test("validateGatewaySharedSecret accepts matching request secrets", () => {
  assert.deepEqual(
    validateGatewaySharedSecret(" expected-secret ", "expected-secret"),
    { ok: true },
  );
});
