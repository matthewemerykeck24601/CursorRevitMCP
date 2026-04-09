/**
 * Mirror of aps-ai-web/src/lib/da-parameter-patch.ts for MCP server bundle.
 * Optional workitem `sharedParameterGuidMap`: only when duplicate definition names exist on elements.
 */

export type DaParameterPatch = {
  externalIds: string[];
  set: Record<string, string | number | boolean | null>;
};

export function mergeParameterPatchesIntoWorkitemArgs(
  workitemArguments: Record<string, unknown>,
  patches: DaParameterPatch[],
): Record<string, unknown> {
  if (patches.length === 0) return workitemArguments;
  const existing = workitemArguments.parameterPatches;
  const prior = Array.isArray(existing)
    ? (existing as DaParameterPatch[])
    : [];
  return {
    ...workitemArguments,
    parameterPatches: [...prior, ...patches],
  };
}

export function parseDaParameterPatchesFromRequest(raw: unknown): DaParameterPatch[] {
  if (!Array.isArray(raw)) return [];
  const out: DaParameterPatch[] = [];
  for (const row of raw) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const ext = r.externalIds;
    const set = r.set;
    if (!Array.isArray(ext) || !set || typeof set !== "object" || Array.isArray(set)) continue;
    const ids = ext
      .filter((x): x is string => typeof x === "string")
      .map((s) => s.trim())
      .filter(Boolean);
    const setObj: Record<string, string | number | boolean | null> = {};
    for (const [k, v] of Object.entries(set as Record<string, unknown>)) {
      if (typeof v === "string" || typeof v === "number" || typeof v === "boolean" || v === null) {
        setObj[k] = v;
      }
    }
    if (ids.length > 0 && Object.keys(setObj).length > 0) {
      out.push({ externalIds: ids, set: setObj });
    }
  }
  return out;
}
