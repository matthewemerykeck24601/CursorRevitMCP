import { env } from "@/lib/env";
import { lookupRecentDaWorkitemByCacheId } from "@/lib/da-recent-workitem-cache";
import {
  extractForgeWorkitemStatusSnippet,
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
  /** Longer audit / validation text for failures and partial runs. */
  da_audit_detail?: string;
  /** APS workitem GET payload excerpt when status is failed/cancelled. */
  forge_status_snippet?: string;
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
  const cacheHint = String(o.cache_id ?? "").trim();
  const workitem_id =
    String(o.workitem_id ?? "").trim() ||
    (cacheHint ? lookupRecentDaWorkitemByCacheId(cacheHint)?.workitem_id ?? "" : "");
  if (!workitem_id) {
    return {
      success: false,
      workitem_id: "",
      attempts_used: 0,
      polls: [],
      message: cacheHint
        ? `workitem_id is required (no recent job registered for cache_id ${cacheHint}).`
        : "workitem_id is required (pass lastDaJob or discovery.pending_workitem, or cache_id for server fallback).",
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
    24,
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
          message: sum.one_liner,
          da_audit_summary: { one_liner: sum.one_liner },
          ...(sum.detail ? { da_audit_detail: sum.detail } : {}),
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
    const line = `Design Automation finished with status "${last.status}".`;
    const forgeSnip = extractForgeWorkitemStatusSnippet(last.raw);
    let auditOne: string | undefined;
    let auditDetail: string | undefined;
    if (last.reportUrl) {
      const sum = await fetchDaAuditReportSummary(last.reportUrl);
      if (sum) {
        auditOne = sum.one_liner;
        auditDetail = sum.detail;
      }
    }
    const combinedMessage = [auditOne ?? line, forgeSnip].filter(Boolean).join(" — ");
    return {
      success: false,
      workitem_id,
      status: last.status,
      attempts_used,
      polls,
      message: combinedMessage,
      ...(auditOne ? { da_audit_summary: { one_liner: auditOne } } : {}),
      ...(auditDetail || forgeSnip
        ? {
            da_audit_detail: [auditDetail, forgeSnip].filter(Boolean).join(" | "),
          }
        : {}),
      ...(forgeSnip ? { forge_status_snippet: forgeSnip } : {}),
      execution_assistant_hint: `${auditOne ?? line}${forgeSnip ? ` APS: ${forgeSnip.slice(0, 280)}` : ""} State the real outcome from audit_report if present; never imply the model changed if audit says otherwise.`,
    };
  }

  return {
    success: true,
    workitem_id,
    status: last?.status,
    attempts_used,
    polls,
    message: `Job is still in progress (${last?.status ?? "pending"}; ~${elapsedApproxSec}s elapsed across ${attempts_used} poll(s)).`,
    execution_assistant_hint: `Job is still running. Reply in ONE short sentence with status "${last?.status ?? "pending"}" and ~${elapsedApproxSec}s elapsed; say the user can ask again in a few seconds for an update — do not imply failure.`,
  };
}
