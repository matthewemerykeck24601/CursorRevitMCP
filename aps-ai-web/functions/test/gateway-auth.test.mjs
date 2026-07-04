import assert from "node:assert/strict";
import test from "node:test";

import gatewayModule from "../lib/index.js";

const { validateAiGatewaySecret } = gatewayModule;

test("AI gateway rejects requests when shared secret is missing", () => {
  assert.deepEqual(validateAiGatewaySecret("", "anything"), {
    ok: false,
    status: 500,
    error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
  });
});

test("AI gateway rejects missing or incorrect request secrets", () => {
  assert.deepEqual(validateAiGatewaySecret("expected", undefined), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateAiGatewaySecret("expected", "wrong"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("AI gateway accepts the configured request secret", () => {
  assert.deepEqual(validateAiGatewaySecret("expected", "expected"), {
    ok: true,
  });
});
