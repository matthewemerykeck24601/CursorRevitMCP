export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 500; error: string };

function normalizeSharedSecret(value: string | undefined): string {
  return (value ?? "").trim();
}

function timingSafeStringEquals(left: string, right: string): boolean {
  const maxLength = Math.max(left.length, right.length);
  let mismatch = left.length ^ right.length;

  for (let idx = 0; idx < maxLength; idx += 1) {
    const leftCode = idx < left.length ? left.charCodeAt(idx) : 0;
    const rightCode = idx < right.length ? right.charCodeAt(idx) : 0;
    mismatch |= leftCode ^ rightCode;
  }

  return mismatch === 0;
}

export function authorizeGatewayRequest(
  receivedSecret: string | undefined,
  expectedSecret: string | undefined,
): GatewayAuthResult {
  const expected = normalizeSharedSecret(expectedSecret);
  if (!expected) {
    return {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const received = normalizeSharedSecret(receivedSecret);
  if (!received || !timingSafeStringEquals(received, expected)) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
