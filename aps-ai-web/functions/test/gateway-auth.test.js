const assert = require("node:assert/strict");
const { describe, it } = require("node:test");

const { validateGatewaySecret } = require("../lib/gateway-auth.js");

describe("validateGatewaySecret", () => {
  it("fails closed when the configured shared secret is missing", () => {
    assert.deepEqual(validateGatewaySecret("", "caller-secret"), {
      ok: false,
      status: 503,
      error: "AI gateway shared secret is not configured.",
    });
  });

  it("rejects requests without the expected shared secret", () => {
    assert.deepEqual(validateGatewaySecret("expected", "wrong"), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
  });

  it("accepts matching shared secrets after trimming storage whitespace", () => {
    assert.deepEqual(validateGatewaySecret(" expected\n", "expected "), {
      ok: true,
    });
  });
});
