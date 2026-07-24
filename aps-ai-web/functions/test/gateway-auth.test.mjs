import assert from "node:assert/strict";
import { describe, it } from "node:test";

import { authorizeGatewayRequest } from "../lib/gatewayAuth.js";

describe("authorizeGatewayRequest", () => {
  it("fails closed when the configured secret is missing", () => {
    for (const expectedSecret of [undefined, "", "   "]) {
      assert.deepEqual(authorizeGatewayRequest(expectedSecret, "attacker"), {
        ok: false,
        status: 503,
        error: "AI gateway shared secret is not configured.",
      });
    }
  });

  it("rejects missing and incorrect request secrets", () => {
    for (const receivedSecret of [undefined, "", "wrong-secret"]) {
      assert.deepEqual(
        authorizeGatewayRequest("expected-secret", receivedSecret),
        {
          ok: false,
          status: 401,
          error: "Unauthorized AI gateway request.",
        },
      );
    }
  });

  it("accepts only the configured request secret", () => {
    assert.deepEqual(
      authorizeGatewayRequest("expected-secret", "expected-secret"),
      { ok: true },
    );
  });
});
