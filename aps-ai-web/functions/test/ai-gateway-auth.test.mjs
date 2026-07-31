import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { validateAiGatewaySecret } from "../lib/index.js";

describe("validateAiGatewaySecret", () => {
  it("fails closed when the configured shared secret is missing", () => {
    const missing = validateAiGatewaySecret("", "attacker-supplied");
    const blank = validateAiGatewaySecret("   ", "attacker-supplied");

    assert.deepEqual(missing, {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    });
    assert.deepEqual(blank, {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    });
  });

  it("rejects requests without the expected shared secret", () => {
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

  it("accepts requests with the configured shared secret", () => {
    assert.deepEqual(validateAiGatewaySecret("expected", "expected"), { ok: true });
  });
});
