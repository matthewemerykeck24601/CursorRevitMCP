import { env } from "@/lib/env";

const APS_BASE = "https://developer.api.autodesk.com";

type AppToken = {
  access_token: string;
  expires_in: number;
};

function assertCredentials() {
  if (!env.apsClientId || !env.apsClientSecret) {
    throw new Error("Missing APS client credentials.");
  }
}

function sanitizeBucketKey(input: string): string {
  return input
    .toLowerCase()
    .replace(/[^a-z0-9._-]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 128);
}

export function getDesignFilesBucketKey(): string {
  const configured = (env.apsDesignFilesBucket || "").trim();
  if (configured) return sanitizeBucketKey(configured);
  const fallback = `aps-${env.apsClientId || "local"}-productdesigncalcs`;
  return sanitizeBucketKey(fallback);
}

async function getAppToken(): Promise<string> {
  assertCredentials();
  const body = new URLSearchParams({
    client_id: env.apsClientId,
    client_secret: env.apsClientSecret,
    grant_type: "client_credentials",
    scope: "bucket:create bucket:read data:read data:write data:create",
  });
  const response = await fetch(`${APS_BASE}/authentication/v2/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
    cache: "no-store",
  });
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Failed to get app token (${response.status}): ${text}`);
  }
  const json = (await response.json()) as AppToken;
  return json.access_token;
}

async function ensureBucket(bucketKey: string, token: string): Promise<void> {
  const details = await fetch(
    `${APS_BASE}/oss/v2/buckets/${encodeURIComponent(bucketKey)}/details`,
    {
      method: "GET",
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
    },
  );
  if (details.ok) return;
  if (details.status !== 404) {
    const text = await details.text();
    throw new Error(`Bucket check failed (${details.status}): ${text}`);
  }

  const createResponse = await fetch(`${APS_BASE}/oss/v2/buckets`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      bucketKey,
      policyKey: "transient",
    }),
    cache: "no-store",
  });
  if (!createResponse.ok) {
    const text = await createResponse.text();
    throw new Error(`Bucket create failed (${createResponse.status}): ${text}`);
  }
}

export async function uploadProjectJsonObject(
  bucketKey: string,
  objectKey: string,
  payload: unknown,
): Promise<void> {
  const token = await getAppToken();
  await ensureBucket(bucketKey, token);
  const response = await fetch(
    `${APS_BASE}/oss/v2/buckets/${encodeURIComponent(bucketKey)}/objects/${encodeURIComponent(objectKey)}`,
    {
      method: "PUT",
      headers: {
        Authorization: `Bearer ${token}`,
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Object upload failed (${response.status}): ${text}`);
  }
}

export async function listProjectObjects(
  bucketKey: string,
  prefix: string,
): Promise<string[]> {
  const token = await getAppToken();
  await ensureBucket(bucketKey, token);
  const response = await fetch(
    `${APS_BASE}/oss/v2/buckets/${encodeURIComponent(bucketKey)}/objects?limit=100&prefix=${encodeURIComponent(prefix)}`,
    {
      method: "GET",
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Object list failed (${response.status}): ${text}`);
  }
  const json = (await response.json()) as {
    items?: Array<{ objectKey?: string }>;
  };
  return (json.items ?? [])
    .map((i) => i.objectKey ?? "")
    .filter((k) => k.length > 0);
}

export async function fetchProjectObjectJson<T>(
  bucketKey: string,
  objectKey: string,
): Promise<T> {
  const token = await getAppToken();
  const response = await fetch(
    `${APS_BASE}/oss/v2/buckets/${encodeURIComponent(bucketKey)}/objects/${encodeURIComponent(objectKey)}`,
    {
      method: "GET",
      headers: { Authorization: `Bearer ${token}` },
      cache: "no-store",
    },
  );
  if (!response.ok) {
    const text = await response.text();
    throw new Error(`Object fetch failed (${response.status}): ${text}`);
  }
  return (await response.json()) as T;
}
