const assert = require("node:assert/strict");
const test = require("node:test");

const {
  getRequiredGatewaySecret,
  isGatewaySecretMatch,
} = require("../lib/gateway-auth.js");

test("gateway auth rejects missing configured secrets", () => {
  assert.equal(getRequiredGatewaySecret(undefined), null);
  assert.equal(getRequiredGatewaySecret(""), null);
  assert.equal(getRequiredGatewaySecret("   "), null);

  assert.equal(isGatewaySecretMatch(undefined, "received-secret"), false);
  assert.equal(isGatewaySecretMatch("", "received-secret"), false);
  assert.equal(isGatewaySecretMatch("   ", "received-secret"), false);
});

test("gateway auth requires the received header to match exactly", () => {
  assert.equal(getRequiredGatewaySecret(" configured-secret "), "configured-secret");

  assert.equal(isGatewaySecretMatch("configured-secret", undefined), false);
  assert.equal(isGatewaySecretMatch("configured-secret", ""), false);
  assert.equal(isGatewaySecretMatch("configured-secret", "wrong-secret"), false);
  assert.equal(isGatewaySecretMatch("configured-secret", "configured-secret "), false);
  assert.equal(isGatewaySecretMatch("configured-secret", "configured-secret"), true);
});
