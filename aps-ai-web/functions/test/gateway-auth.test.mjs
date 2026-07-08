import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { authorizeGatewayRequest } from "../lib/gatewayAuth.js";

describe("authorizeGatewayRequest", () => {
  it("fails closed when the expected secret is missing", () => {
    assert.deepEqual(authorizeGatewayRequest("", "sent-secret"), {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    });
    assert.deepEqual(authorizeGatewayRequest("   ", "sent-secret"), {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    });
  });

  it("rejects missing or mismatched request secrets", () => {
    assert.deepEqual(authorizeGatewayRequest("expected-secret", undefined), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
    assert.deepEqual(authorizeGatewayRequest("expected-secret", "wrong-secret"), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
  });

  it("accepts requests with the configured shared secret", () => {
    assert.deepEqual(authorizeGatewayRequest("expected-secret", "expected-secret"), {
      ok: true,
    });
  });
});
