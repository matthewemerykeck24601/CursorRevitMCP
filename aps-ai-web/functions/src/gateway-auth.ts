export const AI_GATEWAY_SECRET_HEADER = "x-ai-gateway-secret";

export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 500; error: string };

export function validateGatewaySharedSecret(
  expectedSecret: string | null | undefined,
  receivedSecret: string | null | undefined,
): GatewayAuthResult {
  const expected = (expectedSecret ?? "").trim();
  if (!expected) {
    return {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const received = (receivedSecret ?? "").trim();
  if (!received || received !== expected) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
