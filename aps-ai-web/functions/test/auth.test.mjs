import assert from "node:assert/strict";
import test from "node:test";

import auth from "../lib/auth.js";

const { validateGatewaySecret } = auth;

test("validateGatewaySecret fails closed when the configured secret is blank", () => {
  assert.deepEqual(validateGatewaySecret("", "anything"), {
    ok: false,
    status: 503,
    error: "AI gateway auth secret is not configured.",
  });
  assert.deepEqual(validateGatewaySecret("   ", "anything"), {
    ok: false,
    status: 503,
    error: "AI gateway auth secret is not configured.",
  });
});

test("validateGatewaySecret rejects missing or mismatched request secrets", () => {
  assert.deepEqual(validateGatewaySecret("expected", undefined), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(validateGatewaySecret("expected", "wrong"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("validateGatewaySecret accepts the matching request secret", () => {
  assert.deepEqual(validateGatewaySecret(" expected ", "expected"), {
    ok: true,
    expectedSecret: "expected",
  });
});
