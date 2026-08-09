import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { verifyGatewaySharedSecret } from "./index";

describe("verifyGatewaySharedSecret", () => {
  it("fails closed when the configured shared secret is missing", () => {
    assert.deepEqual(verifyGatewaySharedSecret("", "provided"), {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    });
    assert.deepEqual(verifyGatewaySharedSecret("   ", "provided"), {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    });
  });

  it("rejects requests with no or mismatched shared secret header", () => {
    assert.deepEqual(verifyGatewaySharedSecret("expected", undefined), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
    assert.deepEqual(verifyGatewaySharedSecret("expected", "wrong"), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
  });

  it("accepts matching shared secret headers", () => {
    assert.deepEqual(verifyGatewaySharedSecret("expected", "expected"), {
      ok: true,
    });
  });
});
