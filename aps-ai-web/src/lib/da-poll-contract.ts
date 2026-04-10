import { env } from "@/lib/env";
import {
  fetchDaAuditReportSummary,
  pollDaWorkitemWithRetries,
  type DaWorkitemPollResult,
} from "@/lib/da-workitems";

export type PollDesignAutomationStatusPayload = {
  success: boolean;
  workitem_id: string;
  status?: string;
  attempts_used: number;
  polls: DaWorkitemPollResult[];
  da_audit_summary?: { one_liner: string };
  execution_assistant_hint?: string;
  message?: string;
  reportUrl_missing?: boolean;
  audit_fetch_failed?: boolean;
};

/**
 * Poll Design Automation GET /workitems/{id} (with bounded retries) and
 * optionally fetch audit_report.json from reportUrl.
 */
export async function pollDesignAutomationStatusContract(
  raw: unknown,
): Promise<PollDesignAutomationStatusPayload> {
  const o = raw && typeof raw === "object" ? (raw as Record<string, unknown>) : {};
  const workitem_id = String(o.workitem_id ?? "").trim();
  if (!workitem_id) {
    return {
      success: false,
      workitem_id: "",
      attempts_used: 0,
      polls: [],
      message: "workitem_id is required.",
    };
  }
  if (workitem_id.startsWith("da-stub")) {
    return {
      success: false,
      workitem_id,
      attempts_used: 0,
      polls: [],
      message: "Stub workitem — no cloud job to poll.",
    };
  }
  if (env.daEnabled !== "true") {
    return {
      success: false,
      workitem_id,
      attempts_used: 0,
      polls: [],
      message: "DA_ENABLED is not true — polling is unavailable.",
    };
  }

  const maxAttempts = Math.min(
    4,
    Math.max(
      1,
      typeof o.max_attempts === "number" && Number.isFinite(o.max_attempts)
        ? Math.trunc(o.max_attempts)
        : env.daPollMaxAttempts,
    ),
  );
  const delayBetweenMs =
    typeof o.delay_ms_between_attempts === "number" &&
    Number.isFinite(o.delay_ms_between_attempts)
      ? Math.min(15_000, Math.max(0, Math.trunc(o.delay_ms_between_attempts)))
      : env.daPollDelayMs;

  const { polls, last, attempts_used } = await pollDaWorkitemWithRetries(
    workitem_id,
    { maxAttempts, delayBetweenMs },
  );

  const st = last?.status?.toLowerCase() ?? "";
  const elapsedApproxSec = Math.max(0, (attempts_used - 1) * (delayBetweenMs / 1000));

  if (!last?.ok) {
    return {
      success: false,
      workitem_id,
      attempts_used,
      polls,
      status: last?.status,
      message: "Could not read workitem status from Design Automation.",
      execution_assistant_hint:
        "DA status request failed. One sentence: suggest the user verify APS credentials and DA region; do not invent audit results.",
    };
  }

  if (st === "success") {
    if (last.reportUrl) {
      const sum = await fetchDaAuditReportSummary(last.reportUrl);
      if (sum) {
        return {
          success: true,
          workitem_id,
          status: st,
          attempts_used,
          polls,
          da_audit_summary: { one_liner: sum.one_liner },
          execution_assistant_hint: sum.assistant_hint,
        };
      }
      return {
        success: true,
        workitem_id,
        status: st,
        attempts_used,
        polls,
        reportUrl_missing: false,
        audit_fetch_failed: true,
        message: "Workitem succeeded but audit report could not be read (expired URL or parse error).",
        execution_assistant_hint:
          "DA workitem status is success but audit download failed. One sentence: confirm in Revit/ACC; do not fabricate parameter counts.",
      };
    }
    return {
      success: true,
      workitem_id,
      status: st,
      attempts_used,
      polls,
      reportUrl_missing: true,
      message: "Workitem succeeded; no reportUrl on this poll (activity may omit report output).",
      execution_assistant_hint:
        "DA reports success without reportUrl. One short sentence: job finished — user should sync Revit or check ACC for changes.",
    };
  }

  if (st.startsWith("failed") || st === "cancelled") {
    return {
      success: false,
      workitem_id,
      status: last.status,
      attempts_used,
      polls,
      message: `Workitem ${st}.`,
      execution_assistant_hint: `DA workitem ended with status ${last.status}. One factual sentence; no audit success claims.`,
    };
  }

  return {
    success: true,
    workitem_id,
    status: last?.status,
    attempts_used,
    polls,
    message: `Still ${last?.status ?? "pending"} after ${attempts_used} poll(s).`,
    execution_assistant_hint: `Job is still running (~${elapsedApproxSec}s between checks). Reply in ONE short sentence; say the user can ask again shortly for an update — do not say still preparing or imply failure.`,
  };
}
