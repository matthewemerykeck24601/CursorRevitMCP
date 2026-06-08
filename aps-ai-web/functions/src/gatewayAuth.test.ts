import assert from "node:assert/strict";
import { describe, it } from "node:test";
import { authorizeGatewayRequest } from "./gatewayAuth";

describe("authorizeGatewayRequest", () => {
  it("fails closed when the expected shared secret is missing", () => {
    assert.deepEqual(authorizeGatewayRequest("provided", undefined), {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    });
  });

  it("fails closed when the expected shared secret is blank", () => {
    assert.deepEqual(authorizeGatewayRequest("provided", "   "), {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    });
  });

  it("rejects requests without the shared secret header", () => {
    assert.deepEqual(authorizeGatewayRequest(undefined, "expected"), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
  });

  it("rejects requests with the wrong shared secret", () => {
    assert.deepEqual(authorizeGatewayRequest("wrong", "expected"), {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    });
  });

  it("authorizes requests with the matching shared secret", () => {
    assert.deepEqual(authorizeGatewayRequest("expected", "expected"), { ok: true });
  });

  it("ignores accidental surrounding whitespace in configured and received secrets", () => {
    assert.deepEqual(authorizeGatewayRequest(" expected ", " expected "), { ok: true });
  });
});
