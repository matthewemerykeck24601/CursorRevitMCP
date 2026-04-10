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

export type DaParameterUpdateRow = {
  externalIds: string[];
  paramName: string;
  action: "clear" | "set" | "toggle";
  value?: string | number | boolean | null;
};

export function parseDaParameterUpdatesFromRequest(raw: unknown): DaParameterUpdateRow[] {
  if (!Array.isArray(raw)) return [];
  const out: DaParameterUpdateRow[] = [];
  for (const row of raw) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const ext = r.externalIds;
    const paramName = typeof r.paramName === "string" ? r.paramName.trim() : "";
    const actionRaw = typeof r.action === "string" ? r.action.trim().toLowerCase() : "";
    if (!Array.isArray(ext) || !paramName) continue;
    if (actionRaw !== "clear" && actionRaw !== "set" && actionRaw !== "toggle") continue;
    const ids = ext
      .filter((x): x is string => typeof x === "string")
      .map((s) => s.trim())
      .filter(Boolean);
    if (ids.length === 0) continue;
    const action = actionRaw as DaParameterUpdateRow["action"];
    const v = r.value;
    const value =
      typeof v === "string" || typeof v === "number" || typeof v === "boolean" || v === null
        ? v
        : undefined;
    out.push({ externalIds: ids, paramName, action, value });
  }
  return out;
}

/** Mirror of aps-ai-web: `cached_selection` + `updates` → `parameter_updates` rows. */
export function expandParameterUpdatesFromCachedSelection(
  cached_selection: unknown,
  updates: unknown,
): DaParameterUpdateRow[] {
  if (!cached_selection || typeof cached_selection !== "object" || Array.isArray(cached_selection)) {
    return [];
  }
  if (!Array.isArray(updates) || updates.length === 0) return [];

  const sel = cached_selection as Record<string, unknown>;
  const extRaw = sel.externalIds;
  if (!Array.isArray(extRaw)) return [];
  const externalIds = extRaw
    .filter((x): x is string => typeof x === "string")
    .map((s) => s.trim())
    .filter(Boolean);
  if (externalIds.length === 0) return [];

  const out: DaParameterUpdateRow[] = [];
  for (const row of updates) {
    if (!row || typeof row !== "object") continue;
    const r = row as Record<string, unknown>;
    const paramName = typeof r.paramName === "string" ? r.paramName.trim() : "";
    const actionRaw = typeof r.action === "string" ? r.action.trim().toLowerCase() : "";
    if (!paramName) continue;
    if (actionRaw !== "clear" && actionRaw !== "set" && actionRaw !== "toggle") continue;
    const action = actionRaw as DaParameterUpdateRow["action"];
    const v = r.value;
    const value =
      typeof v === "string" || typeof v === "number" || typeof v === "boolean" || v === null
        ? v
        : undefined;
    out.push({ externalIds, paramName, action, value });
  }
  return out;
}
