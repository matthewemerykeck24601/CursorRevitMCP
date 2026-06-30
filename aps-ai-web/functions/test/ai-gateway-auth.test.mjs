import test from "node:test";
import assert from "node:assert/strict";
import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { authorizeAiGatewayRequest } = require("../lib/index.js");

test("AI gateway auth fails closed when shared secret is missing", () => {
  assert.deepEqual(authorizeAiGatewayRequest("", undefined), {
    ok: false,
    status: 503,
    error: "AI gateway shared secret is not configured.",
  });
  assert.deepEqual(authorizeAiGatewayRequest("   ", "anything"), {
    ok: false,
    status: 503,
    error: "AI gateway shared secret is not configured.",
  });
});

test("AI gateway auth rejects missing or incorrect caller secret", () => {
  assert.deepEqual(authorizeAiGatewayRequest("expected", undefined), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
  assert.deepEqual(authorizeAiGatewayRequest("expected", "wrong"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("AI gateway auth accepts matching shared secret", () => {
  assert.deepEqual(authorizeAiGatewayRequest(" expected ", " expected "), {
    ok: true,
  });
});
