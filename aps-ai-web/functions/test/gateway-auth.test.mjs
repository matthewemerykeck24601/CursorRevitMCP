import assert from "node:assert/strict";
import test from "node:test";

import {
  isAuthorizedGatewayRequest,
  normalizeGatewaySharedSecret,
} from "../lib/index.js";

test("normalizeGatewaySharedSecret trims configured secrets", () => {
  assert.equal(normalizeGatewaySharedSecret("  configured-secret  "), "configured-secret");
  assert.equal(normalizeGatewaySharedSecret("   "), "");
  assert.equal(normalizeGatewaySharedSecret(undefined), "");
});

test("isAuthorizedGatewayRequest fails closed without a configured secret", () => {
  assert.equal(isAuthorizedGatewayRequest(undefined, "anything"), false);
  assert.equal(isAuthorizedGatewayRequest("", "anything"), false);
  assert.equal(isAuthorizedGatewayRequest("   ", "anything"), false);
});

test("isAuthorizedGatewayRequest rejects missing or incorrect request secrets", () => {
  assert.equal(isAuthorizedGatewayRequest("configured-secret", undefined), false);
  assert.equal(isAuthorizedGatewayRequest("configured-secret", ""), false);
  assert.equal(isAuthorizedGatewayRequest("configured-secret", "wrong-secret"), false);
});

test("isAuthorizedGatewayRequest accepts the exact configured request secret", () => {
  assert.equal(isAuthorizedGatewayRequest(" configured-secret ", "configured-secret"), true);
});
