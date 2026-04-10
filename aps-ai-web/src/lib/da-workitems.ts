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

export type DaWorkitemPollResult = {
  ok: boolean;
  status?: string;
  progress?: string;
  reportUrl?: string;
  raw?: unknown;
};

/**
 * GET workitem status (Design Automation v3). Use after submit for a quick poll;
 * often still `pending`/`inprogress` on first call.
 */
function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

/**
 * Poll GET /workitems/:id up to `maxAttempts` times, waiting `delayBetweenMs`
 * between attempts while status is pending/inprogress (rate-limit friendly).
 */
export async function pollDaWorkitemWithRetries(
  workitemId: string,
  options?: { maxAttempts?: number; delayBetweenMs?: number },
): Promise<{
  polls: DaWorkitemPollResult[];
  last: DaWorkitemPollResult | null;
  attempts_used: number;
}> {
  const maxAttempts = Math.min(4, Math.max(1, options?.maxAttempts ?? 2));
  const delayBetweenMs = Math.min(
    15_000,
    Math.max(0, options?.delayBetweenMs ?? 2000),
  );
  const polls: DaWorkitemPollResult[] = [];
  for (let i = 0; i < maxAttempts; i++) {
    const r = await fetchDaWorkitemStatus(workitemId);
    polls.push(r);
    const st = r.status?.toLowerCase() ?? "";
    if (st === "success" || st.startsWith("failed") || st === "cancelled") {
      return { polls, last: r, attempts_used: i + 1 };
    }
    if (i < maxAttempts - 1 && delayBetweenMs > 0) {
      await sleep(delayBetweenMs);
    }
  }
  const last = polls.length > 0 ? polls[polls.length - 1]! : null;
  return { polls, last, attempts_used: polls.length };
}

export async function fetchDaWorkitemStatus(
  workitemId: string,
): Promise<DaWorkitemPollResult> {
  if (env.daEnabled !== "true" || !workitemId || workitemId.startsWith("da-stub")) {
    return { ok: false };
  }
  try {
    const token = await getDesignAutomationAccessToken();
    const base = daBaseUrl();
    const res = await fetch(
      `${base}/workitems/${encodeURIComponent(workitemId)}`,
      {
        headers: { Authorization: `Bearer ${token}` },
        cache: "no-store",
      },
    );
    const text = await res.text();
    let raw: unknown;
    try {
      raw = JSON.parse(text) as unknown;
    } catch {
      raw = { rawText: text };
    }
    if (!res.ok) {
      return { ok: false, raw };
    }
    const o = raw as {
      status?: unknown;
      progress?: unknown;
      reportUrl?: unknown;
    };
    return {
      ok: true,
      status: typeof o.status === "string" ? o.status : undefined,
      progress: typeof o.progress === "string" ? o.progress : undefined,
      reportUrl: typeof o.reportUrl === "string" ? o.reportUrl : undefined,
      raw,
    };
  } catch (e) {
    return {
      ok: false,
      raw: { error: e instanceof Error ? e.message : String(e) },
    };
  }
}

type DaAuditJson = {
  resolved_operation?: string;
  payload_operation?: string;
  modify?: {
    parameter_actions_ok?: number;
    parameter_actions_failed?: number;
  };
  summary?: {
    unique_elements_resolved_modify?: number;
    parameter_actions_ok?: number;
    parameter_actions_failed?: number;
  };
  post_run?: { summary_line?: string };
  swc?: { status?: string };
  log?: Array<{ text?: string; level?: string }>;
};

function inferPrimaryParamFromAuditLog(
  log: DaAuditJson["log"],
): { param: string; action: string } | null {
  if (!Array.isArray(log)) return null;
  for (const row of log) {
    if (!row || typeof row !== "object") continue;
    const text = String((row as { text?: unknown }).text ?? "");
    const m = text.match(/Modify:\s*(\w+)\s+'([^']+)'/i);
    if (m?.[1] && m?.[2]) {
      return { action: m[1].toLowerCase(), param: m[2] };
    }
  }
  return null;
}

/**
 * Build chat-oriented lines from parsed audit_report.json (Revit add-in).
 */
export function summarizeDaAuditJson(
  json: unknown,
): { one_liner: string; assistant_hint: string } | null {
  if (!json || typeof json !== "object") return null;
  const j = json as DaAuditJson;
  const ok = j.modify?.parameter_actions_ok ?? j.summary?.parameter_actions_ok ?? 0;
  const fail =
    j.modify?.parameter_actions_failed ?? j.summary?.parameter_actions_failed ?? 0;
  const unique = j.summary?.unique_elements_resolved_modify ?? 0;
  const swcStatus = j.swc?.status?.trim() ?? "";
  const postLine =
    typeof j.post_run?.summary_line === "string"
      ? j.post_run.summary_line.trim()
      : "";
  const syncNote =
    swcStatus.toLowerCase() === "success"
      ? " Sync completed successfully."
      : swcStatus && swcStatus.toLowerCase() !== "skipped"
        ? ` SWC: ${swcStatus}.`
        : "";
  const op = (j.resolved_operation ?? j.payload_operation ?? "").toLowerCase();
  const fromLog = inferPrimaryParamFromAuditLog(j.log);
  let one_liner: string;
  if (fromLog && unique > 0 && fail === 0) {
    const verb =
      fromLog.action === "clear"
        ? "Cleared"
        : fromLog.action === "set"
          ? "Updated"
          : "Applied changes to";
    one_liner =
      `${verb} ${fromLog.param} on ${unique} element(s) (${ok} action(s)).${syncNote}`.trim();
  } else if (op.includes("modify") && unique > 0) {
    one_liner =
      `Parameter updates on ${unique} element(s): ${ok} succeeded, ${fail} failed.${syncNote}`.trim();
  } else {
    one_liner =
      unique > 0
        ? `Parameter actions: ${ok} succeeded, ${fail} failed, across ${unique} element(s).${syncNote}`.trim()
        : (postLine ||
            `Parameter actions: ${ok} succeeded, ${fail} failed.${syncNote}`).trim();
  }
  const assistant_hint = `Design Automation audit is available. Reply in exactly ONE short sentence using: ${one_liner} Do not ask for confirmation; do not say still preparing.`;
  return { one_liner, assistant_hint };
}

/**
 * Fetch audit_report.json from DA workitem reportUrl (after status success).
 * Returns a short user-facing line + hint for the finalizer.
 */
export async function fetchDaAuditReportSummary(
  reportUrl: string,
): Promise<{ one_liner: string; assistant_hint: string } | null> {
  if (!reportUrl || !reportUrl.startsWith("http")) return null;
  try {
    const res = await fetch(reportUrl, { cache: "no-store" });
    if (!res.ok) return null;
    const json: unknown = await res.json();
    return summarizeDaAuditJson(json);
  } catch {
    return null;
  }
}
