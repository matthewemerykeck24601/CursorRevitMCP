import assert from "node:assert/strict";
import test from "node:test";
import { hasAnyAiProviderKeyForEnv } from "@/lib/env";

function config(
  overrides: Partial<Parameters<typeof hasAnyAiProviderKeyForEnv>[0]> = {},
): Parameters<typeof hasAnyAiProviderKeyForEnv>[0] {
  return {
    aiGatewayMode: "direct",
    aiGatewayFunctionUrl: "",
    aiGatewaySharedSecret: "",
    aiOpenAiKey: "",
    aiXaiKey: "",
    ...overrides,
  };
}

test("Firebase gateway availability requires both URL and shared secret", () => {
  assert.equal(
    hasAnyAiProviderKeyForEnv(
      config({
        aiGatewayMode: "firebase_functions",
        aiGatewayFunctionUrl: "https://example.test/aiGateway",
      }),
    ),
    false,
  );
  assert.equal(
    hasAnyAiProviderKeyForEnv(
      config({
        aiGatewayMode: "firebase_functions",
        aiGatewayFunctionUrl: "https://example.test/aiGateway",
        aiGatewaySharedSecret: "secret",
      }),
    ),
    true,
  );
});

test("direct keys cannot mask an incomplete Firebase gateway configuration", () => {
  assert.equal(
    hasAnyAiProviderKeyForEnv(
      config({
        aiGatewayMode: "firebase_functions",
        aiGatewayFunctionUrl: "https://example.test/aiGateway",
        aiXaiKey: "direct-key",
      }),
    ),
    false,
  );
});

test("direct mode accepts either provider key", () => {
  assert.equal(hasAnyAiProviderKeyForEnv(config({ aiXaiKey: "xai-key" })), true);
  assert.equal(
    hasAnyAiProviderKeyForEnv(config({ aiOpenAiKey: "openai-key" })),
    true,
  );
});
