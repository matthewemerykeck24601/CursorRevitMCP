const assert = require("node:assert/strict");
const test = require("node:test");

const { getAiGatewayAuthFailure } = require("../lib/index.js");

test("AI gateway auth fails closed when the shared secret is missing", () => {
  assert.equal(
    getAiGatewayAuthFailure("", "client-secret"),
    "AI_GATEWAY_SHARED_SECRET secret is not configured.",
  );
  assert.equal(
    getAiGatewayAuthFailure("   ", "client-secret"),
    "AI_GATEWAY_SHARED_SECRET secret is not configured.",
  );
});

test("AI gateway auth rejects missing or mismatched request secrets", () => {
  assert.equal(
    getAiGatewayAuthFailure("server-secret", undefined),
    "Unauthorized AI gateway request.",
  );
  assert.equal(
    getAiGatewayAuthFailure("server-secret", "other-secret"),
    "Unauthorized AI gateway request.",
  );
});

test("AI gateway auth accepts matching request secret", () => {
  assert.equal(getAiGatewayAuthFailure("server-secret", "server-secret"), null);
});
