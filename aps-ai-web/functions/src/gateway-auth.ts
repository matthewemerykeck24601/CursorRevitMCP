import { timingSafeEqual } from "node:crypto";

export type GatewaySecretValidation =
  | { ok: true }
  | { ok: false; status: 401 | 503; error: string };

function normalizeSecret(value: unknown): string {
  return typeof value === "string" ? value.trim() : "";
}

function secretsMatch(expected: string, received: string): boolean {
  const expectedBytes = Buffer.from(expected);
  const receivedBytes = Buffer.from(received);
  return (
    expectedBytes.length === receivedBytes.length &&
    timingSafeEqual(expectedBytes, receivedBytes)
  );
}

export function validateGatewaySecret(
  expectedSecretValue: unknown,
  receivedSecretValue: unknown,
): GatewaySecretValidation {
  const expectedSecret = normalizeSecret(expectedSecretValue);
  if (!expectedSecret) {
    return {
      ok: false,
      status: 503,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const receivedSecret = normalizeSecret(receivedSecretValue);
  if (!receivedSecret || !secretsMatch(expectedSecret, receivedSecret)) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
