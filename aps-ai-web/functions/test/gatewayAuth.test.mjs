import assert from "node:assert/strict";
import test from "node:test";

import { authorizeGatewayRequest } from "../lib/gatewayAuth.js";

test("gateway auth fails closed when expected shared secret is missing", () => {
  assert.deepEqual(authorizeGatewayRequest("client-secret", undefined), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });

  assert.deepEqual(authorizeGatewayRequest("client-secret", "   "), {
    ok: false,
    status: 500,
    error: "AI gateway shared secret is not configured.",
  });
});

test("gateway auth rejects absent or mismatched request secrets", () => {
  assert.deepEqual(authorizeGatewayRequest(undefined, "server-secret"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });

  assert.deepEqual(authorizeGatewayRequest("wrong-secret", "server-secret"), {
    ok: false,
    status: 401,
    error: "Unauthorized AI gateway request.",
  });
});

test("gateway auth accepts matching request secret", () => {
  assert.deepEqual(authorizeGatewayRequest(" server-secret ", "server-secret"), {
    ok: true,
  });
});
