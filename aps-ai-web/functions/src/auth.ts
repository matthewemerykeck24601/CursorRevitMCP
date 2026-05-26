import { timingSafeEqual } from "node:crypto";

export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 500; error: string };

function trimSecret(value: string | undefined): string {
  return (value ?? "").trim();
}

function secretsMatch(expected: string, received: string): boolean {
  const expectedBuffer = Buffer.from(expected);
  const receivedBuffer = Buffer.from(received);
  return (
    expectedBuffer.length === receivedBuffer.length &&
    timingSafeEqual(expectedBuffer, receivedBuffer)
  );
}

export function validateGatewaySharedSecret(
  expectedSecret: string | undefined,
  receivedSecret: string | undefined,
): GatewayAuthResult {
  const expected = trimSecret(expectedSecret);
  if (!expected) {
    return {
      ok: false,
      status: 500,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const received = trimSecret(receivedSecret);
  if (!received || !secretsMatch(expected, received)) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
