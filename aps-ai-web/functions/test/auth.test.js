const assert = require("node:assert/strict");
const test = require("node:test");

const { validateAiGatewaySecret } = require("../lib/auth.js");

test("rejects requests when the configured shared secret is missing", () => {
  assert.deepEqual(validateAiGatewaySecret("", "client-secret"), {
    status: 500,
    error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
  });
});

test("rejects requests without the expected shared secret", () => {
  assert.deepEqual(validateAiGatewaySecret("server-secret", undefined), {
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateAiGatewaySecret("server-secret", "wrong-secret"), {
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("accepts requests with the matching shared secret", () => {
  assert.equal(validateAiGatewaySecret("server-secret", "server-secret"), null);
});
