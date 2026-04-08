/**
 * Minimal DA v3 client for MCP (uses process.env). Mirror of Next da-workitems.ts.
 */
const APS_BASE = "https://developer.api.autodesk.com";

function env(name: string, fallback = ""): string {
  return process.env[name]?.trim() ?? fallback;
}

export async function getDesignAutomationAccessTokenMcp(): Promise<string> {
  const id = env("APS_CLIENT_ID");
  const secret = env("APS_CLIENT_SECRET");
  if (!id || !secret) {
    throw new Error("APS_CLIENT_ID and APS_CLIENT_SECRET required for Design Automation.");
  }
  const body = new URLSearchParams({
    client_id: id,
    client_secret: secret,
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
  if (!json.access_token) throw new Error("DA token missing access_token.");
  return json.access_token;
}

function daBaseUrlMcp(): string {
  const region = env("DA_REGION", "us-east").toLowerCase();
  if (region === "emea" || region === "eu") {
    return "https://developer.api.autodesk.com/da/eu/v3";
  }
  if (region === "apac" || region === "asia") {
    return "https://developer.api.autodesk.com/da/apac/v3";
  }
  return "https://developer.api.autodesk.com/da/us-east/v3";
}

export async function submitMarkUpdateWorkitemMcp(params: {
  workitemArguments: Record<string, unknown>;
}): Promise<{ id: string; status?: string; raw?: unknown } | null> {
  if (env("DA_ENABLED", "").toLowerCase() !== "true") {
    return null;
  }
  const activityId = env("DA_ACTIVITY_ID");
  if (!activityId) {
    throw new Error("DA_ACTIVITY_ID required when DA_ENABLED=true.");
  }
  const token = await getDesignAutomationAccessTokenMcp();
  const base = daBaseUrlMcp();
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
