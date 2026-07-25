export type GatewaySecretValidation =
  | { ok: true; expectedSecret: string }
  | { ok: false; status: 401 | 503; error: string };

export function validateGatewaySecret(
  expectedSecret: string,
  receivedSecret: string | undefined,
): GatewaySecretValidation {
  const expected = expectedSecret.trim();
  if (!expected) {
    return {
      ok: false,
      status: 503,
      error: "AI gateway auth secret is not configured.",
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

  return { ok: true, expectedSecret: expected };
}
