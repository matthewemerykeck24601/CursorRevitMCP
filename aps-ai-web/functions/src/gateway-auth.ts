export type GatewayAuthResult =
  | { ok: true }
  | {
      ok: false;
      status: 401 | 500;
      error: string;
    };

export function validateGatewaySecret(
  expectedSecret: string | undefined,
  receivedSecret: string | undefined,
): GatewayAuthResult {
  if (!expectedSecret) {
    return {
      ok: false,
      status: 500,
      error: "AI_GATEWAY_SHARED_SECRET secret is not configured.",
    };
  }

  if (!receivedSecret || receivedSecret !== expectedSecret) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
