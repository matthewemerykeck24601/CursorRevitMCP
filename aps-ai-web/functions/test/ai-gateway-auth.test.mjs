import assert from "node:assert/strict";
import test from "node:test";

import authModule from "../lib/ai-gateway-auth.js";

const { validateAiGatewaySecret } = authModule;

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
