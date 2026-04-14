/**
 * Browser-only debug for /api/chat and viewer actions.
 * Enable any of:
 * - URL: ?aiDebug=1
 * - DevTools: localStorage.setItem("APS_AI_DEBUG", "1") then reload
 * - Build: NEXT_PUBLIC_AI_DEBUG=true
 */
export function isAiBrowserDebugEnabled(): boolean {
  if (typeof window === "undefined") return false;
  if (process.env.NEXT_PUBLIC_AI_DEBUG === "true") return true;
  try {
    if (window.localStorage.getItem("APS_AI_DEBUG") === "1") return true;
    if (new URLSearchParams(window.location.search).get("aiDebug") === "1") {
      return true;
    }
  } catch {
    /* private mode / SSR edge */
  }
  return false;
}

export function aiBrowserDebug(label: string, data: unknown): void {
  if (!isAiBrowserDebugEnabled()) return;
  console.debug(`[aps-ai] ${label}`, data);
}
