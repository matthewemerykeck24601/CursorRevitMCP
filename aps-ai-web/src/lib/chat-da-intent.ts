/**
 * Chat-side intent for Design Automation execution (aps-ai-web route + Alice).
 * Does not run in Revit; only decides when to auto-submit workitems.
 */

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

/** Map user text to updates[] for modify_parameters (best-effort). */
export function inferDaUpdatesFromUserMessage(
  userMessage: string,
): InferredDaUpdate[] {
  const m = userMessage.toLowerCase();
  if (/\bclear\b/.test(m)) {
    if (/\buniformat\b/.test(m)) {
      return [{ paramName: "UNIFORMAT_CODE", action: "clear" }];
    }
    return [{ paramName: "CONTROL_MARK", action: "clear" }];
  }
  const setMark = m.match(
    /(?:set|assign|put)\s+(?:control[_\s]?mark\s+)?(?:to\s+)?['"]?([A-Za-z0-9._-]+)['"]?/i,
  );
  if (setMark?.[1] && /\bmark\b/.test(m)) {
    return [
      { paramName: "CONTROL_MARK", action: "set", value: setMark[1].trim() },
    ];
  }
  if (/\bproceed\b/.test(m)) {
    return [{ paramName: "CONTROL_MARK", action: "clear" }];
  }
  return [];
}
