/**
 * Chat-side intent for Design Automation execution (aps-ai-web route + Alice).
 * Does not run in Revit; only decides when to auto-submit workitems.
 */

import { hasExecutableDirectEdits } from "@/lib/da-parameter-patch";

export type InferredDaUpdate = {
  paramName: string;
  action: "clear" | "set" | "toggle";
  value?: string;
};

/** When discovery exists, treat common edit verbs as immediate execution (no second confirm). */
export function shouldAutoExecuteDesignAutomation(
  userMessage: string,
  hasActiveDiscovery: boolean,
): boolean {
  if (!hasActiveDiscovery) return false;
  const m = userMessage.toLowerCase();
  if (/\b(don't|do not|never|cancel|abort|wait|hold off|skip)\b/.test(m)) {
    return false;
  }
  if (/^\s*(are you sure|really)\s*\??\s*$/i.test(m.trim())) {
    return false;
  }
  if (
    /\b(analyze|analysis|verify|preview|sameness|grouping)\b/.test(m) &&
    !/\b(clear|set|update|chang|proceed|apply|submit|sync)\b/.test(m)
  ) {
    return false;
  }
  const hasEditVerb =
    /\bclear\b/.test(m) ||
    /\bset\b/.test(m) ||
    /\bupdate\b/.test(m) ||
    /\bchang\w+/.test(m) ||
    /\b(proceed|apply|submit)\b/.test(m) ||
    /\bmark\w*\b/.test(m);
  const syncOnly =
    /\bsync\b/.test(m) &&
    !hasEditVerb &&
    !/\b(proceed|apply|submit)\b/.test(m);
  if (syncOnly) return false;
  return hasEditVerb || /\bsync\b/.test(m);
}

/** Keep only rows the DA expander accepts (paramName + clear|set|toggle). */
export function sanitizeDaUpdatesArray(raw: unknown): InferredDaUpdate[] {
  if (!Array.isArray(raw)) return [];
  const out: InferredDaUpdate[] = [];
  for (const row of raw) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const paramNameRaw =
      typeof r.paramName === "string"
        ? r.paramName.trim()
        : typeof r.parameter === "string"
          ? r.parameter.trim()
          : typeof r.name === "string"
            ? r.name.trim()
            : "";
    const actionRaw =
      typeof r.action === "string" ? r.action.trim().toLowerCase() : "";
    if (!paramNameRaw) continue;
    if (actionRaw !== "clear" && actionRaw !== "set" && actionRaw !== "toggle") {
      continue;
    }
    const action = actionRaw as InferredDaUpdate["action"];
    const v = r.value;
    const value =
      typeof v === "string" || typeof v === "number" || typeof v === "boolean"
        ? String(v)
        : undefined;
    const u: InferredDaUpdate = { paramName: paramNameRaw, action };
    if (action === "set" && value !== undefined) u.value = value;
    out.push(u);
  }
  return out;
}

/** Phrases that imply CONTROL_MARK (or named param) should be cleared. */
export function impliesControlMarkClear(userMessage: string): boolean {
  const t = userMessage.toLowerCase();
  if (/\bclear\b.*\bcontrol[_\s]?mark\b|\bcontrol[_\s]?mark\b.*\bclear\b/.test(t)) {
    return true;
  }
  if (
    /\b(clear|remove|delete|strip)\b/.test(t) &&
    /\b(marks?|mark values?|piece marks?)\b/.test(t)
  ) {
    return true;
  }
  if (/\b(clear|remove)\b/.test(t) && /\bmarking\b/.test(t)) return true;
  return false;
}

/** Map user text to updates[] for modify_parameters (best-effort). */
export function inferDaUpdatesFromUserMessage(
  userMessage: string,
): InferredDaUpdate[] {
  const m = userMessage.toLowerCase();
  const explicitCm = userMessage.match(
    /\bclear\b\s+(?:the\s+)?(?:param(?:eter)?\s+)?CONTROL_MARK\b/i,
  );
  if (explicitCm) {
    return [{ paramName: "CONTROL_MARK", action: "clear" }];
  }
  const namedClear = userMessage.match(
    /\bclear\b\s+(?:the\s+)?(?:param(?:eter)?\s+)?([A-Z0-9_]{2,})\b/i,
  );
  if (namedClear?.[1] && !/^(THE|ALL|ANY)\b/i.test(namedClear[1])) {
    return [{ paramName: namedClear[1].trim(), action: "clear" }];
  }
  if (impliesControlMarkClear(userMessage)) {
    return [{ paramName: "CONTROL_MARK", action: "clear" }];
  }
  if (/\bclear\b/.test(m)) {
    if (/\buniformat\b/.test(m)) {
      return [{ paramName: "UNIFORMAT_CODE", action: "clear" }];
    }
    return [];
  }
  const setMark = m.match(
    /(?:set|assign|put)\s+(?:control[_\s]?mark\s+)?(?:to\s+)?['"]?([A-Za-z0-9._-]+)['"]?/i,
  );
  if (setMark?.[1] && /\bmark\b/.test(m)) {
    return [
      { paramName: "CONTROL_MARK", action: "set", value: setMark[1].trim() },
    ];
  }
  const setParam = userMessage.match(
    /\bset\b\s+([A-Z0-9_]+)\s+to\s+['"]?([^'"\n]+?)['"]?(?:\s|$)/i,
  );
  if (setParam?.[1] && setParam[2]) {
    return [
      {
        paramName: setParam[1].trim(),
        action: "set",
        value: setParam[2].trim(),
      },
    ];
  }
  return [];
}

/**
 * User asked to change parameters in vague terms but we could not build updates[].
 * Caller should ask for clarification instead of submitting skip_analysis with no edits.
 */
export function modifyParametersNeedsClarification(userMessage: string): boolean {
  const m = userMessage.toLowerCase();
  if (impliesControlMarkClear(userMessage)) return false;
  if (/\bclear\b/.test(m)) {
    return inferDaUpdatesFromUserMessage(userMessage).length === 0;
  }
  if (/\b(set|assign|update|chang\w+)\b/.test(m) && /\bparam|mark|value\b/.test(m)) {
    return inferDaUpdatesFromUserMessage(userMessage).length === 0;
  }
  return false;
}

/**
 * For skip_analysis + discovery: attach cached_selection, normalize updates[], operation.
 * Returns a blocked reason for externalContext when we should not call the DA contract.
 */
export function augmentSkipAnalysisTriggerArgs(
  userMessage: string,
  base: Record<string, unknown>,
  daCachedSelection: Record<string, unknown> | null,
): { blocked?: string; userHint?: string } {
  if (base.skip_analysis !== true) return {};
  if (!daCachedSelection) return {};

  const ext = daCachedSelection.externalIds;
  const extLen = Array.isArray(ext)
    ? ext.filter(
        (x): x is string => typeof x === "string" && x.trim().length > 0,
      ).length
    : 0;
  if (extLen === 0) return {};

  const opNorm = String(base.operation ?? "").trim().toLowerCase();
  const lockedOps = new Set([
    "run_mark_analysis",
    "apply_marks",
    "apply_marks_and_modify",
    "clear_cache",
  ]);
  if (lockedOps.has(opNorm)) {
    return {};
  }

  base.cached_selection = daCachedSelection;
  base.operation = "modify_parameters";

  const plannerUpdatesRaw = base.updates;
  let resolved = sanitizeDaUpdatesArray(plannerUpdatesRaw);
  if (resolved.length === 0) {
    resolved = inferDaUpdatesFromUserMessage(userMessage);
  }
  if (resolved.length === 0 && impliesControlMarkClear(userMessage)) {
    resolved = [{ paramName: "CONTROL_MARK", action: "clear" }];
  }

  if (resolved.length > 0) {
    base.updates = resolved;
    return {};
  }

  if (hasExecutableDirectEdits(base as Record<string, unknown>)) {
    return {};
  }

  if (modifyParametersNeedsClarification(userMessage)) {
    return {
      blocked:
        "DA_MODIFY_PARAMETERS_BLOCKED: Say which parameter to clear, set, or toggle (e.g. CONTROL_MARK), or use wording like “clear the marks”.",
      userHint:
        "I need to know which parameter to change. Example: “clear CONTROL_MARK on the selection” or “set CONTROL_MARK to CLA-001”.",
    };
  }

  return {
    blocked:
      "DA_MODIFY_PARAMETERS_BLOCKED: No valid updates[] after merging discovery; omitting workitem.",
    userHint:
      "I could not build a parameter action for that message. Try “clear the marks” or “clear CONTROL_MARK”.",
  };
}

/** Status / result follow-ups: poll DA when client sent lastDaJob.workitem_id. */
export function shouldAutoPollDaFollowUp(userMessage: string): boolean {
  const m = userMessage.toLowerCase().trim();
  if (m.length === 0) return false;
  if (/\b(don't|do not|cancel)\b/.test(m)) return false;
  return (
    /\b(status|update\??|result|finished|done|complete|progress|how('?s| is) it)\b/.test(
      m,
    ) ||
    /\b(did it work|did that work|any news|what happened|audit|outcome)\b/.test(
      m,
    ) ||
    /^(still )?(running|going)\??$/.test(m) ||
    /\b(check|poll)( again)?\b/.test(m)
  );
}
