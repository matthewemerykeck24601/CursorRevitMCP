import assert from "node:assert/strict";
import test from "node:test";

import { isAuthorizedGatewayRequest } from "../lib/index.js";

test("AI gateway authorization fails closed without a configured secret", () => {
  assert.equal(isAuthorizedGatewayRequest("", undefined), false);
  assert.equal(isAuthorizedGatewayRequest("   ", "anything"), false);
});

test("AI gateway authorization requires the exact shared secret", () => {
  assert.equal(isAuthorizedGatewayRequest("shared-secret", undefined), false);
  assert.equal(isAuthorizedGatewayRequest("shared-secret", ""), false);
  assert.equal(isAuthorizedGatewayRequest("shared-secret", "wrong"), false);
  assert.equal(isAuthorizedGatewayRequest("shared-secret", " shared-secret "), false);
  assert.equal(isAuthorizedGatewayRequest("shared-secret", "shared-secret"), true);
});
