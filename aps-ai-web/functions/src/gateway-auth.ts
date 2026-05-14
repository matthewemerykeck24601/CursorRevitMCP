export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 500; error: string };

export function validateGatewaySharedSecret(
  expectedSecret: string | undefined | null,
  receivedSecret: string | undefined | null,
): GatewayAuthResult {
  const expected = typeof expectedSecret === "string" ? expectedSecret.trim() : "";
  if (!expected) {
    return {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const received = typeof receivedSecret === "string" ? receivedSecret.trim() : "";
  if (!received || received !== expected) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
