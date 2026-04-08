import { env } from "@/lib/env";

const APS_BASE = "https://developer.api.autodesk.com";

export type DaWorkitemSubmitResult = {
  id: string;
  status?: string;
  raw?: unknown;
};

/**
 * Two-legged OAuth for Design Automation (scope code:all).
 */
export async function getDesignAutomationAccessToken(): Promise<string> {
  if (!env.apsClientId || !env.apsClientSecret) {
    throw new Error("APS_CLIENT_ID and APS_CLIENT_SECRET are required for Design Automation.");
  }
  const body = new URLSearchParams({
    client_id: env.apsClientId,
    client_secret: env.apsClientSecret,
    grant_type: "client_credentials",
    scope: "code:all",
  });
  const res = await fetch(`${APS_BASE}/authentication/v2/token`, {
    method: "POST",
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    body,
    cache: "no-store",
  });
  if (!res.ok) {
    const t = await res.text();
    throw new Error(`DA token failed (${res.status}): ${t}`);
  }
  const json = (await res.json()) as { access_token?: string };
  if (!json.access_token) throw new Error("DA token response missing access_token.");
  return json.access_token;
}

function daBaseUrl(): string {
  const region = (env.daRegion || "us-east").trim().toLowerCase();
  const host =
    region === "emea"
      ? "https://developer.api.autodesk.com/da/eu/v3"
      : region === "apac" || region === "asia"
        ? "https://developer.api.autodesk.com/da/apac/v3"
        : "https://developer.api.autodesk.com/da/us-east/v3";
  return host;
}

/**
 * Submit a DA v3 workitem. Activity must define argument names matching `arguments`.
 * Returns null if DA is disabled via env.
 */
export async function submitMarkUpdateWorkitem(params: {
  workitemArguments: Record<string, unknown>;
}): Promise<DaWorkitemSubmitResult | null> {
  if (env.daEnabled !== "true") {
    return null;
  }
  const activityId = env.daActivityId;
  if (!activityId) {
    throw new Error("DA_ACTIVITY_ID is required when DA_ENABLED=true.");
  }

  const token = await getDesignAutomationAccessToken();
  const base = daBaseUrl();

  /** Generic payload: align keys with your AppBundle / Activity definition. */
  const body = {
    activityId,
    arguments: {
      payload: {
        text: JSON.stringify(params.workitemArguments),
      },
    },
  };

  const res = await fetch(`${base}/workitems`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
    cache: "no-store",
  });

  const text = await res.text();
  let raw: unknown;
  try {
    raw = JSON.parse(text) as unknown;
  } catch {
    raw = { rawText: text };
  }

  if (!res.ok) {
    throw new Error(`DA workitem failed (${res.status}): ${text}`);
  }

  const id =
    typeof raw === "object" &&
    raw !== null &&
    "id" in raw &&
    typeof (raw as { id: unknown }).id === "string"
      ? (raw as { id: string }).id
      : `da-${Date.now()}`;

  const status =
    typeof raw === "object" &&
    raw !== null &&
    "status" in raw &&
    typeof (raw as { status: unknown }).status === "string"
      ? (raw as { status: string }).status
      : "submitted";

  return { id, status, raw };
}
