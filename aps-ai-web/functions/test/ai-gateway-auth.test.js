const assert = require("node:assert/strict");
const test = require("node:test");

const { validateAiGatewaySecret } = require("../lib/ai-gateway-auth.js");

test("validateAiGatewaySecret rejects missing server shared secret", () => {
  assert.deepEqual(validateAiGatewaySecret("", "client-secret"), {
    status: 503,
    error: "AI gateway shared secret is not configured.",
  });
  assert.deepEqual(validateAiGatewaySecret("   ", "client-secret"), {
    status: 503,
    error: "AI gateway shared secret is not configured.",
  });
});

test("validateAiGatewaySecret rejects missing or incorrect client secret", () => {
  assert.deepEqual(validateAiGatewaySecret("server-secret", undefined), {
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateAiGatewaySecret("server-secret", "wrong-secret"), {
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("validateAiGatewaySecret accepts matching shared secrets", () => {
  assert.equal(validateAiGatewaySecret(" server-secret ", "server-secret"), null);
  assert.equal(validateAiGatewaySecret("server-secret", " server-secret "), null);
});
