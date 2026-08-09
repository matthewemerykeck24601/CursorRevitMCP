import assert from "node:assert/strict";
import test from "node:test";

import authModule from "../lib/auth.js";

const { validateGatewaySharedSecret } = authModule;

test("validateGatewaySharedSecret fails closed when expected secret is missing", () => {
  assert.deepEqual(validateGatewaySharedSecret("", "sent-secret"), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });
});

test("validateGatewaySharedSecret rejects missing or mismatched request secrets", () => {
  assert.deepEqual(validateGatewaySharedSecret("expected-secret", ""), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateGatewaySharedSecret("expected-secret", "wrong-secret"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("validateGatewaySharedSecret accepts matching request secrets", () => {
  assert.deepEqual(
    validateGatewaySharedSecret(" expected-secret ", "expected-secret"),
    { ok: true },
  );
});
