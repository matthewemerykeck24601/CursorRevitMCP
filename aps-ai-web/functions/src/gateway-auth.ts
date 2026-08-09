function normalizeSecret(secret: string | null | undefined): string {
  return (secret ?? "").trim();
}

function constantTimeEquals(left: string, right: string): boolean {
  let mismatch = left.length ^ right.length;
  const length = Math.max(left.length, right.length);

  for (let index = 0; index < length; index += 1) {
    mismatch |= (left.charCodeAt(index) || 0) ^ (right.charCodeAt(index) || 0);
  }

  return mismatch === 0;
}

export function getRequiredGatewaySecret(secret: string | null | undefined): string | null {
  const normalized = normalizeSecret(secret);
  return normalized ? normalized : null;
}

export function isGatewaySecretMatch(
  configuredSecret: string | null | undefined,
  receivedSecret: string | null | undefined,
): boolean {
  const expectedSecret = getRequiredGatewaySecret(configuredSecret);
  if (!expectedSecret || !receivedSecret) return false;

  return constantTimeEquals(expectedSecret, receivedSecret);
}
