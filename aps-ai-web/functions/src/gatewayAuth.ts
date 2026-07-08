export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 500; error: string };

export function authorizeGatewayRequest(
  expectedSecret: string | undefined,
  receivedSecret: string | undefined,
): GatewayAuthResult {
  const expected = String(expectedSecret ?? "").trim();
  if (!expected) {
    return {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const received = String(receivedSecret ?? "");
  if (!received || received !== expected) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
