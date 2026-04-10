/**
 * In-process fallback when the client did not persist lastDaJob.workitem_id.
 * Keyed by discovery cache_id from the DA payload (best-effort; not durable across server restarts).
 */

type Entry = { workitem_id: string; submitted_at: string };

const byCacheId = new Map<string, Entry>();
const MAX_ENTRIES = 200;

export function registerDaWorkitemForCacheId(
  cacheId: string | undefined,
  workitemId: string,
  submittedAt: string,
): void {
  const cid = (cacheId ?? "").trim();
  const wid = (workitemId ?? "").trim();
  if (!cid || !wid || wid.startsWith("da-stub")) return;
  byCacheId.set(cid, { workitem_id: wid, submitted_at: submittedAt });
  while (byCacheId.size > MAX_ENTRIES) {
    const first = byCacheId.keys().next().value;
    if (first === undefined) break;
    byCacheId.delete(first);
  }
}

export function lookupRecentDaWorkitemByCacheId(
  cacheId: string | undefined,
): Entry | undefined {
  const cid = (cacheId ?? "").trim();
  if (!cid) return undefined;
  return byCacheId.get(cid);
}
