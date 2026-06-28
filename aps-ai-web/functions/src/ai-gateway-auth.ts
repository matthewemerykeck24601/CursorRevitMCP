export type AiGatewayAuthFailure = {
  status: 401 | 503;
  error: string;
};

export function validateAiGatewaySecret(
  expectedSecretRaw: string | undefined,
  receivedSecretRaw: string | undefined,
): AiGatewayAuthFailure | null {
  const expectedSecret = (expectedSecretRaw ?? "").trim();
  if (!expectedSecret) {
    return {
      status: 503,
      error: "AI gateway shared secret is not configured.",
    };
  }

  const receivedSecret = (receivedSecretRaw ?? "").trim();
  if (!receivedSecret || receivedSecret !== expectedSecret) {
    return {
      status: 401,
      error: "Unauthorized AI gateway request.",
    };
  }

  return null;
}
