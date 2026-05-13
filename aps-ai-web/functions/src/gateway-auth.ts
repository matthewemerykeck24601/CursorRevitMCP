import { timingSafeEqual } from "node:crypto";

export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 503; error: string };

export function validateGatewaySecret(
  configuredSecret: string,
  receivedSecret: string,
): GatewayAuthResult {
  if (!configuredSecret.trim()) {
    return {
      ok: false,
      status: 503,
      error: "AI gateway shared secret is not configured.",
    };
  }

  if (!receivedSecret) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  const configuredBuffer = Buffer.from(configuredSecret);
  const receivedBuffer = Buffer.from(receivedSecret);
  const matches =
    configuredBuffer.length === receivedBuffer.length &&
    timingSafeEqual(configuredBuffer, receivedBuffer);

  if (!matches) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
