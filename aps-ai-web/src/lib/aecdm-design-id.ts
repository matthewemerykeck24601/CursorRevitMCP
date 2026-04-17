function tryBase64Decode(input: string): string {
  try {
    const normalized = input.replace(/-/g, "+").replace(/_/g, "/");
    const padLen = normalized.length % 4 === 0 ? 0 : 4 - (normalized.length % 4);
    const padded = normalized + "=".repeat(padLen);
    return Buffer.from(padded, "base64").toString("utf8").trim();
  } catch {
    return "";
  }
}

function extractFromUrn(urn: string): string {
  // Preferred: explicit lineage URN.
  const lineageMatch = urn.match(/:dm\.lineage:([^?]+)$/i);
  if (lineageMatch?.[1]) return lineageMatch[1];

  // Common viewer decode result: urn:...:fs.file:vf.<lineage>?version=...
  const vfMatch = urn.match(/:fs\.file:vf\.([^?]+)(?:\?|$)/i);
  if (vfMatch?.[1]) return vfMatch[1];

  // Last-chance fallback for URN-like strings.
  const tail = urn.split(":").pop()?.trim() ?? "";
  return tail;
}

/**
 * Resolve AECDM design id from any model identifier the app may pass:
 * - raw lineage id
 * - dm.lineage URN
 * - version URN (vf.<lineage>?version=)
 * - viewer URN (base64url-encoded version URN)
 */
export function resolveAecdmDesignId(modelIdentifier: string): string {
  const raw = modelIdentifier.trim();
  if (!raw) return "";

  if (raw.startsWith("urn:")) {
    return extractFromUrn(raw);
  }

  const decoded = tryBase64Decode(raw);
  if (decoded.startsWith("urn:")) {
    return extractFromUrn(decoded);
  }

  // Already a likely lineage/design id.
  return raw;
}

