import { timingSafeEqual } from "node:crypto";

export type GatewayAuthResult =
  | { ok: true }
  | { ok: false; status: 401 | 503; error: string };

export function authorizeGatewayRequest(
  expectedSecret: string | undefined,
  receivedSecret: string | undefined,
): GatewayAuthResult {
  const expected = (expectedSecret ?? "").trim();
  if (!expected) {
    return {
      ok: false,
      status: 503,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const expectedBuffer = Buffer.from(expected);
  const receivedBuffer = Buffer.from(receivedSecret ?? "");
  if (
    receivedBuffer.length !== expectedBuffer.length ||
    !timingSafeEqual(receivedBuffer, expectedBuffer)
  ) {
    return {
      ok: false,
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return { ok: true };
}
