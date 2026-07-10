import test from "node:test";
import assert from "node:assert/strict";
import { hasAnyAiProviderKeyForEnv } from "@/lib/env";

function makeConfig(
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

test("firebase_functions gateway mode requires both URL and shared secret", () => {
  assert.equal(
    hasAnyAiProviderKeyForEnv(
      makeConfig({
        aiGatewayMode: "firebase_functions",
        aiGatewayFunctionUrl: "https://example.test/aiGateway",
      }),
    ),
    false,
  );

  assert.equal(
    hasAnyAiProviderKeyForEnv(
      makeConfig({
        aiGatewayMode: "firebase_functions",
        aiGatewayFunctionUrl: "https://example.test/aiGateway",
        aiGatewaySharedSecret: "gateway-secret",
      }),
    ),
    true,
  );
});

test("direct AI mode still accepts direct provider keys", () => {
  assert.equal(hasAnyAiProviderKeyForEnv(makeConfig({ aiXaiKey: "xai-key" })), true);
  assert.equal(
    hasAnyAiProviderKeyForEnv(makeConfig({ aiOpenAiKey: "openai-key" })),
    true,
  );
});
