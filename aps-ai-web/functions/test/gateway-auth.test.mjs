import assert from "node:assert/strict";
import test from "node:test";

import indexModule from "../lib/index.js";

const { getGatewayAuthError } = indexModule;

test("gateway auth fails closed when shared secret is not configured", () => {
  const expectedError = "AI_GATEWAY_SHARED_SECRET secret is not configured.";

  assert.equal(getGatewayAuthError(undefined, "client-secret"), expectedError);
  assert.equal(getGatewayAuthError("", "client-secret"), expectedError);
  assert.equal(getGatewayAuthError("   ", "client-secret"), expectedError);
});

test("gateway auth rejects missing or mismatched client secret", () => {
  const expectedError = "Unauthorized AI gateway request.";

  assert.equal(getGatewayAuthError("server-secret", undefined), expectedError);
  assert.equal(getGatewayAuthError("server-secret", ""), expectedError);
  assert.equal(getGatewayAuthError("server-secret", "wrong-secret"), expectedError);
});

test("gateway auth accepts exact shared secret match", () => {
  assert.equal(getGatewayAuthError("server-secret", "server-secret"), null);
});
