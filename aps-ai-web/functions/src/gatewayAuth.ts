import { timingSafeEqual } from "node:crypto";

export type GatewayAuthorizationResult =
  | { ok: true }
  | { ok: false; status: 401 | 500; error: string };

function secretsMatch(receivedSecret: string, expectedSecret: string): boolean {
  const received = Buffer.from(receivedSecret);
  const expected = Buffer.from(expectedSecret);
  return received.length === expected.length && timingSafeEqual(received, expected);
}

export function authorizeGatewayRequest(
  receivedSecret: string | undefined,
  expectedSecret: string | undefined,
): GatewayAuthorizationResult {
  const configuredSecret = (expectedSecret ?? "").trim();
  if (!configuredSecret) {
    return {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    };
  }

  if (!receivedSecret || !secretsMatch(receivedSecret.trim(), configuredSecret)) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
